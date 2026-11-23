namespace Stage12_DistributedConsistency;

// =========================================================================
// ❌ 反模式示範（任務 12.1）：跨系統「雙寫危機 (Dual-Write Problem)」
// 展示同時寫入「資料庫 (DB)」與「訊息佇列 (RabbitMQ)」時，為何無論怎麼調換順序，
// 都無法避免「資料不一致（幽靈訂單 / 幽靈事件）」的分散式災難！
// =========================================================================

/// <summary>
/// 持久化於訂單資料庫的訂單資料列。
/// </summary>
public sealed record OrderRecord(
    Guid OrderId,
    string CustomerName,
    string ProductName,
    decimal Amount,
    DateTime CreatedAt);

/// <summary>
/// 發布至 RabbitMQ 通知下游服務（庫存、物流、通知）的領域事件。
/// </summary>
public sealed record OrderCreatedEvent(
    Guid EventId,
    Guid OrderId,
    string CustomerName,
    string ProductName,
    decimal Amount,
    DateTime OccurredAt);

/// <summary>
/// 模擬支援本地 ACID Transaction 的關聯式資料庫。
/// </summary>
public sealed class SimulatedOrderDatabase
{
    private readonly List<OrderRecord> _committedOrders = new();

    /// <summary>已成功 Commit 的正式訂單清單。</summary>
    public IReadOnlyList<OrderRecord> CommittedOrders => _committedOrders.AsReadOnly();

    /// <summary>設為 true 可模擬 Commit 階段發生 DB 斷線、死鎖或序列化失敗。</summary>
    public bool FailOnCommit { get; set; }

    public SimulatedDbTransaction BeginTransaction()
    {
        Console.WriteLine("[DB] BEGIN TRANSACTION");
        return new SimulatedDbTransaction(this);
    }

    internal void AppendCommitted(IEnumerable<OrderRecord> records)
    {
        _committedOrders.AddRange(records);
    }

    /// <summary>
    /// 模擬資料庫本地交易物件，僅能管理本機 DB 資料的原子提交與回滾。
    /// </summary>
    public sealed class SimulatedDbTransaction : IDisposable
    {
        private readonly SimulatedOrderDatabase _database;
        private readonly List<OrderRecord> _stagedOrders = new();
        private bool _completed;

        internal SimulatedDbTransaction(SimulatedOrderDatabase database)
        {
            _database = database;
        }

        public void InsertOrder(OrderRecord order)
        {
            if (_completed)
                throw new InvalidOperationException("交易已結束，無法再寫入資料！");

            _stagedOrders.Add(order);
            Console.WriteLine($"[DB] INSERT INTO Orders (Id={order.OrderId.ToString()[..8]}..., Item={order.ProductName}) [暫存未提交]");
        }

        public void Commit()
        {
            if (_completed)
                throw new InvalidOperationException("交易已結束，無法重複 Commit！");

            if (_database.FailOnCommit)
            {
                _completed = true;
                _stagedOrders.Clear();
                Console.WriteLine("[DB] ❌ COMMIT 失敗，本地交易已強制 Rollback！");
                throw new InvalidOperationException("資料庫 Commit 失敗（模擬 DB 斷線 / 死鎖）！");
            }

            _database.AppendCommitted(_stagedOrders);
            _stagedOrders.Clear();
            _completed = true;
            Console.WriteLine("[DB] ✅ COMMIT 成功，訂單已正式寫入資料庫。");
        }

        public void Rollback()
        {
            if (_completed) return;

            _stagedOrders.Clear();
            _completed = true;
            Console.WriteLine("[DB] ↩️ ROLLBACK 完成，暫存資料已撤銷。");
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
/// 模擬外部訊息佇列（RabbitMQ）。一旦事件送出便立即被下游消費者接收，無法透過 DB Transaction 回滾。
/// </summary>
public sealed class SimulatedRabbitMqBroker
{
    private readonly List<OrderCreatedEvent> _publishedEvents = new();

    /// <summary>已成功投遞到 Broker 並被下游消費者接收的事件清單。</summary>
    public IReadOnlyList<OrderCreatedEvent> PublishedEvents => _publishedEvents.AsReadOnly();

    /// <summary>設為 true 可模擬 RabbitMQ 瞬斷、網路逾時或 Broker 不可用。</summary>
    public bool IsDown { get; set; }

    /// <summary>當事件成功送達 Broker 時觸發，模擬下游微服務消費者即時訂閱處理。</summary>
    public event Action<OrderCreatedEvent>? OnEventPublished;

    public void Publish(OrderCreatedEvent domainEvent)
    {
        if (IsDown)
        {
            Console.WriteLine($"[MQ] ❌ Publish 失敗！EventId={domainEvent.EventId.ToString()[..8]}...");
            throw new TimeoutException("RabbitMQ 連線逾時／Broker 暫時不可用！");
        }

        _publishedEvents.Add(domainEvent);
        Console.WriteLine($"[MQ] 📨 已發布 OrderCreatedEvent (OrderId={domainEvent.OrderId.ToString()[..8]}...) ➔ 下游庫存/物流/通知服務已立即消費！");
        OnEventPublished?.Invoke(domainEvent);
    }
}

/// <summary>
/// 反模式 A：先寫 DB 並 Commit，再發 MQ。
/// 危機：DB Commit 後若 MQ 瞬斷或程式崩潰，將留下「資料庫有單、下游完全不知道」的幽靈孤兒訂單。
/// </summary>
public sealed class DbFirstOrderService
{
    private readonly SimulatedOrderDatabase _db;
    private readonly SimulatedRabbitMqBroker _broker;

    public DbFirstOrderService(SimulatedOrderDatabase db, SimulatedRabbitMqBroker broker)
    {
        _db = db;
        _broker = broker;
    }

    public void PlaceOrder(string customerName, string productName, decimal amount)
    {
        var order = new OrderRecord(Guid.NewGuid(), customerName, productName, amount, DateTime.UtcNow);
        var domainEvent = new OrderCreatedEvent(Guid.NewGuid(), order.OrderId, customerName, productName, amount, DateTime.UtcNow);

        using (var tx = _db.BeginTransaction())
        {
            tx.InsertOrder(order);
            tx.Commit();
        }

        // DB 已不可逆地 Commit，此時發 MQ 若失敗就產生「有單無事件」
        _broker.Publish(domainEvent);
    }
}

/// <summary>
/// 反模式 B：先發 MQ，再寫 DB。
/// 危機：MQ 發出後就無法撤回，若後續 DB Commit 失敗，下游已扣庫存並出貨，但 DB 裡根本沒這筆訂單（幽靈事件）。
/// </summary>
public sealed class MqFirstOrderService
{
    private readonly SimulatedOrderDatabase _db;
    private readonly SimulatedRabbitMqBroker _broker;

    public MqFirstOrderService(SimulatedOrderDatabase db, SimulatedRabbitMqBroker broker)
    {
        _db = db;
        _broker = broker;
    }

    public void PlaceOrder(string customerName, string productName, decimal amount)
    {
        var order = new OrderRecord(Guid.NewGuid(), customerName, productName, amount, DateTime.UtcNow);
        var domainEvent = new OrderCreatedEvent(Guid.NewGuid(), order.OrderId, customerName, productName, amount, DateTime.UtcNow);

        // 先對外部 Broker 發射不可撤回的訊息
        _broker.Publish(domainEvent);

        using var tx = _db.BeginTransaction();
        tx.InsertOrder(order);
        tx.Commit();
    }
}

/// <summary>
/// 反模式 C：把發 MQ 塞進 DB Transaction 裡面，幻想 Rollback 能順便撤回訊息。
/// 危機：外部 MQ 不受本地 DB Rollback 管轄（依然產生幽靈事件），且將外部網路 I/O 放進交易中會嚴重拉長 DB 鎖表時間！
/// </summary>
public sealed class PublishInsideTransactionOrderService
{
    private readonly SimulatedOrderDatabase _db;
    private readonly SimulatedRabbitMqBroker _broker;

    public PublishInsideTransactionOrderService(SimulatedOrderDatabase db, SimulatedRabbitMqBroker broker)
    {
        _db = db;
        _broker = broker;
    }

    public void PlaceOrder(string customerName, string productName, decimal amount)
    {
        var order = new OrderRecord(Guid.NewGuid(), customerName, productName, amount, DateTime.UtcNow);
        var domainEvent = new OrderCreatedEvent(Guid.NewGuid(), order.OrderId, customerName, productName, amount, DateTime.UtcNow);

        using var tx = _db.BeginTransaction();
        try
        {
            tx.InsertOrder(order);

            // 在 DB 交易尚未結束前就呼叫外部網路 I/O 發送事件
            _broker.Publish(domainEvent);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
