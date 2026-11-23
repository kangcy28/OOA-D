using System.Text.Json;

namespace Stage12_DistributedConsistency;

// =========================================================================
// 🛡️ 任務 12.2：Transactional Outbox Pattern + 冪等消費者 (Idempotent Consumer)
// 核心架構：
// 1. 寫入端 (OutboxOrderService)：把「業務資料 (Orders)」與「待發事件 (OutboxMessages)」
//    放進「同一個本地資料庫 ACID Transaction」裡，做到要嘛全成功、要嘛全回滾！
// 2. 投遞端 (OutboxRelayWorker)：背景巡檢 Outbox 表並發布到 RabbitMQ，
//    提供「至少一次投遞 (At-Least-Once Delivery)」保證。
// 3. 消費端 (IdempotentInventoryConsumer)：以 EventId 去重，消化重複投遞，
//    確保下游業務在重試風暴下依然只有「一次有效執行 (Effectively-Once)」。
// =========================================================================

public enum OutboxStatus
{
    Pending,
    Processed
}

/// <summary>
/// Outbox 資料表實體：記錄尚未或已成功投遞至 RabbitMQ 的領域事件快照。
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; }
    public string EventType { get; }
    public string PayloadJson { get; }
    public DateTime OccurredAt { get; }
    public OutboxStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public OutboxMessage(Guid id, string eventType, string payloadJson, DateTime occurredAt)
    {
        Id = id;
        EventType = eventType;
        PayloadJson = payloadJson;
        OccurredAt = occurredAt;
        Status = OutboxStatus.Pending;
    }

    public void MarkAsProcessed()
    {
        Status = OutboxStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }
}

/// <summary>
/// 模擬包含「Orders 業務表」與「OutboxMessages 事件表」的單一關聯式資料庫。
/// 兩張表共享同一個本地 ACID Transaction 邊界。
/// </summary>
public sealed class OutboxOrderDatabase
{
    private readonly List<OrderRecord> _committedOrders = new();
    private readonly List<OutboxMessage> _committedOutbox = new();

    public IReadOnlyList<OrderRecord> CommittedOrders => _committedOrders.AsReadOnly();
    public IReadOnlyList<OutboxMessage> CommittedOutboxMessages => _committedOutbox.AsReadOnly();

    /// <summary>設為 true 可模擬訂單與 Outbox 同時 Commit 時發生資料庫斷線或死鎖。</summary>
    public bool FailOnCommit { get; set; }

    /// <summary>設為 true 可模擬 Relay Worker 發布 MQ 成功後、回寫 Processed 狀態前突然崩潰。</summary>
    public bool FailOnMarkProcessed { get; set; }

    public OutboxDbTransaction BeginTransaction()
    {
        Console.WriteLine("[OutboxDB] BEGIN TRANSACTION");
        return new OutboxDbTransaction(this);
    }

    internal void AppendCommitted(IEnumerable<OrderRecord> orders, IEnumerable<OutboxMessage> outboxMessages)
    {
        _committedOrders.AddRange(orders);
        _committedOutbox.AddRange(outboxMessages);
    }

    /// <summary>
    /// 撈取尚未處理完畢的 Outbox 訊息（依發生時間排序）。
    /// </summary>
    public IReadOnlyList<OutboxMessage> GetPendingOutboxMessages()
    {
        return _committedOutbox
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.OccurredAt)
            .ToList();
    }

    /// <summary>
    /// 將已成功送達 RabbitMQ 的 Outbox 訊息標記為 Processed。
    /// </summary>
    public void MarkOutboxAsProcessed(Guid messageId)
    {
        if (FailOnMarkProcessed)
        {
            throw new IOException("寫入 Processed 狀態前 Relay Worker 突然崩潰斷線！");
        }

        var message = _committedOutbox.FirstOrDefault(m => m.Id == messageId)
            ?? throw new InvalidOperationException($"找不到 OutboxMessage: {messageId}");

        message.MarkAsProcessed();
        Console.WriteLine($"[OutboxDB] ✅ OutboxMessage ({messageId.ToString()[..8]}...) 已標記為 Processed。");
    }

    /// <summary>
    /// 記錄單筆 Outbox 投遞失敗重試次數（保持 Pending 狀態等待下次巡檢重試）。
    /// </summary>
    public void IncrementOutboxRetry(Guid messageId)
    {
        var message = _committedOutbox.FirstOrDefault(m => m.Id == messageId)
            ?? throw new InvalidOperationException($"找不到 OutboxMessage: {messageId}");

        message.IncrementRetry();
        Console.WriteLine($"[OutboxDB] 🔁 OutboxMessage ({messageId.ToString()[..8]}...) RetryCount +1 ➔ 目前為 {message.RetryCount}（維持 Pending）。");
    }

    /// <summary>
    /// 本地資料庫交易：同時保護 Orders 與 OutboxMessages 的原子寫入與回滾。
    /// </summary>
    public sealed class OutboxDbTransaction : IDisposable
    {
        private readonly OutboxOrderDatabase _database;
        private readonly List<OrderRecord> _stagedOrders = new();
        private readonly List<OutboxMessage> _stagedOutbox = new();
        private bool _completed;

        internal OutboxDbTransaction(OutboxOrderDatabase database)
        {
            _database = database;
        }

        public void InsertOrder(OrderRecord order)
        {
            if (_completed)
                throw new InvalidOperationException("交易已結束，無法再寫入訂單！");

            _stagedOrders.Add(order);
            Console.WriteLine($"[OutboxDB] INSERT INTO Orders (Id={order.OrderId.ToString()[..8]}..., Item={order.ProductName}) [暫存未提交]");
        }

        public void InsertOutboxMessage(OutboxMessage message)
        {
            if (_completed)
                throw new InvalidOperationException("交易已結束，無法再寫入 Outbox！");

            _stagedOutbox.Add(message);
            Console.WriteLine($"[OutboxDB] INSERT INTO OutboxMessages (Id={message.Id.ToString()[..8]}..., Type={message.EventType}) [暫存未提交]");
        }

        public void Commit()
        {
            if (_completed)
                throw new InvalidOperationException("交易已結束，無法重複 Commit！");

            if (_database.FailOnCommit)
            {
                _completed = true;
                _stagedOrders.Clear();
                _stagedOutbox.Clear();
                Console.WriteLine("[OutboxDB] ❌ COMMIT 失敗，Orders 與 OutboxMessages 已同步原子 Rollback！");
                throw new InvalidOperationException("資料庫 Commit 失敗（模擬 DB 斷線 / 死鎖）！");
            }

            _database.AppendCommitted(_stagedOrders, _stagedOutbox);
            _stagedOrders.Clear();
            _stagedOutbox.Clear();
            _completed = true;
            Console.WriteLine("[OutboxDB] ✅ COMMIT 成功，Orders 與 OutboxMessages 已同交易原子提交！");
        }

        public void Rollback()
        {
            if (_completed) return;

            _stagedOrders.Clear();
            _stagedOutbox.Clear();
            _completed = true;
            Console.WriteLine("[OutboxDB] ↩️ ROLLBACK 完成，Orders 與 OutboxMessages 暫存已同步撤銷。");
        }

        public void Dispose()
        {
            if (!_completed)
            {
                Rollback();
            }
        }
    }
}

/// <summary>
/// 寫入端服務：不直接依賴 RabbitMQ Broker！
/// 只透過單一 DB 的 ACID Transaction 同時寫入 Orders 與 OutboxMessages。
/// </summary>
public sealed class OutboxOrderService
{
    private readonly OutboxOrderDatabase _db;

    public OutboxOrderService(OutboxOrderDatabase db)
    {
        _db = db;
    }

    public Guid PlaceOrder(string customerName, string productName, decimal amount)
    {
        var now = DateTime.UtcNow;
        var order = new OrderRecord(Guid.NewGuid(), customerName, productName, amount, now);
        var domainEvent = new OrderCreatedEvent(Guid.NewGuid(), order.OrderId, customerName, productName, amount, now);
        var payloadJson = JsonSerializer.Serialize(domainEvent);
        var outboxMessage = new OutboxMessage(
            domainEvent.EventId,
            nameof(OrderCreatedEvent),
            payloadJson,
            domainEvent.OccurredAt);

        using var tx = _db.BeginTransaction();
        try
        {
            tx.InsertOrder(order);
            tx.InsertOutboxMessage(outboxMessage);
            tx.Commit();
            return order.OrderId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}

/// <summary>
/// 背景 Outbox Relay Worker：定期巡檢 Pending 狀態的事件並投遞到 RabbitMQ。
/// 若單筆失敗（MQ 斷線或標記 Processed 前崩潰），不可讓整個 Worker 當掉，需保留 Pending 供下次重試。
/// </summary>
public sealed class OutboxRelayWorker
{
    private readonly OutboxOrderDatabase _db;
    private readonly SimulatedRabbitMqBroker _broker;

    public OutboxRelayWorker(OutboxOrderDatabase db, SimulatedRabbitMqBroker broker)
    {
        _db = db;
        _broker = broker;
    }

    public void ProcessPendingMessagesOnce()
    {
        var pendingMessages = _db.GetPendingOutboxMessages();
        foreach (var msg in pendingMessages)
        {
            var domainEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(msg.PayloadJson)!;
            try
            {
                _broker.Publish(domainEvent);
                _db.MarkOutboxAsProcessed(msg.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RelayWorker] ⚠️ 投遞 OutboxMessage ({msg.Id.ToString()[..8]}...) 失敗：{ex.Message}");
                _db.IncrementOutboxRetry(msg.Id);
            }
        }
    }
}

/// <summary>
/// 冪等性庫存消費者（Idempotent Consumer）：
/// 因為 Outbox + Relay 保證的是「至少一次投遞 (At-Least-Once)」，同一 EventId 可能被重送多次，
/// 下游消費者必須記錄已處理過的 EventId，避免重複扣減庫存！
/// </summary>
public sealed class IdempotentInventoryConsumer
{
    private readonly HashSet<Guid> _processedEventIds = new();

    public int StockDeductedCount { get; private set; }
    public int DuplicateIgnoredCount { get; private set; }

    public void Handle(OrderCreatedEvent domainEvent)
    {
        if (!_processedEventIds.Add(domainEvent.EventId))
        {
            DuplicateIgnoredCount++;
            Console.WriteLine($"[InventoryConsumer] 🛡️ 偵測到重複 EventId ({domainEvent.EventId.ToString()[..8]}...)，冪等略過，不重複扣庫存！");
            return;
        }

        StockDeductedCount++;
        Console.WriteLine($"[InventoryConsumer] 📦 首次消費 EventId ({domainEvent.EventId.ToString()[..8]}...)，成功為商品「{domainEvent.ProductName}」扣減庫存！");
    }
}
