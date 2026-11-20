using System.Collections.Concurrent;
using System.Diagnostics;

namespace Stage10_ConcurrencyAndIdempotency;

/// <summary>
/// 領域聚合根：售票活動（新增 Version 欄位以支援樂觀鎖併發控制）。
/// </summary>
public class TicketEvent
{
    public Guid Id { get; private set; }
    public string EventName { get; private set; }
    public int RemainingTickets { get; private set; }
    public int Version { get; private set; }

    public TicketEvent(string eventName, int initialTickets)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("活動名稱不可為空", nameof(eventName));
        if (initialTickets < 0)
            throw new ArgumentException("初始票數不可為負數", nameof(initialTickets));

        Id = Guid.NewGuid();
        EventName = eventName;
        RemainingTickets = initialTickets;
        Version = 1;
    }

    // 私有還原建構子：供持久化層反序列化還原物件狀態（包含版本號）
    private TicketEvent(Guid id, string eventName, int remainingTickets, int version)
    {
        Id = id;
        EventName = eventName;
        RemainingTickets = remainingTickets;
        Version = version;
    }

    // 模擬 ORM 每次從資料庫讀取時建立獨立記憶體物件複本
    internal TicketEvent Clone() => new TicketEvent(Id, EventName, RemainingTickets, Version);

    // 模擬樂觀鎖寫入成功後，資料庫自動將 Version + 1
    internal TicketEvent WithIncrementedVersion() =>
        new TicketEvent(Id, EventName, RemainingTickets, Version + 1);

    /// <summary>
    /// 聚合根不變條件守門員：確保剩餘票數不會被扣成負數。
    /// </summary>
    public void ReserveOne()
    {
        if (RemainingTickets <= 0)
            throw new InvalidOperationException("票券已售完！");

        RemainingTickets--;
    }
}

/// <summary>
/// 模擬真實資料庫存取層：帶有非同步 I/O 延遲與單筆 SQL 列鎖原子性（_dbEngineLock）。
/// </summary>
public class InMemoryTicketStore
{
    private readonly Dictionary<Guid, TicketEvent> _events = new();
    private readonly object _dbEngineLock = new();

    public void Seed(TicketEvent ticketEvent)
    {
        lock (_dbEngineLock)
        {
            _events[ticketEvent.Id] = ticketEvent.Clone();
        }
    }

    public async Task<TicketEvent?> GetByIdAsync(Guid id)
    {
        // 模擬真實資料庫 SELECT 的網路與磁碟 I/O 延遲
        await Task.Delay(Random.Shared.Next(10, 30));
        lock (_dbEngineLock)
        {
            return _events.TryGetValue(id, out var e) ? e.Clone() : null;
        }
    }

    public async Task SaveAsync(TicketEvent ticketEvent)
    {
        // 模擬無版本檢查的普通 UPDATE（直接覆寫）
        await Task.Delay(Random.Shared.Next(10, 30));
        lock (_dbEngineLock)
        {
            _events[ticketEvent.Id] = ticketEvent.Clone();
        }
    }

    /// <summary>
    /// 模擬 SQL 樂觀鎖原子更新：
    /// UPDATE TicketEvents SET RemainingTickets = @newQty, Version = Version + 1
    /// WHERE Id = @id AND Version = @expectedVersion
    /// </summary>
    public async Task<bool> TrySaveWithVersionAsync(TicketEvent ticketEvent, int expectedVersion)
    {
        await Task.Delay(Random.Shared.Next(10, 30));
        lock (_dbEngineLock)
        {
            if (!_events.TryGetValue(ticketEvent.Id, out var currentInDb) ||
                currentInDb.Version != expectedVersion)
            {
                return false;
            }

            _events[ticketEvent.Id] = ticketEvent.WithIncrementedVersion().Clone();
            return true;
        }
    }

    public int PeekRemainingTickets(Guid id)
    {
        lock (_dbEngineLock)
        {
            return _events[id].RemainingTickets;
        }
    }

    public int PeekVersion(Guid id)
    {
        lock (_dbEngineLock)
        {
            return _events[id].Version;
        }
    }
}

/// <summary>
/// 實驗 1：未加任何併發控制的搶票服務（保留用於對照組）。
/// </summary>
public class UnsafeTicketBookingService
{
    private readonly InMemoryTicketStore _store;

    public ConcurrentBag<string> SuccessfulBuyers { get; } = new();

    public UnsafeTicketBookingService(InMemoryTicketStore store)
    {
        _store = store;
    }

    public async Task<bool> TryBookTicketAsync(Guid eventId, string buyerName)
    {
        var ticketEvent = await _store.GetByIdAsync(eventId);
        if (ticketEvent == null) return false;

        try
        {
            ticketEvent.ReserveOne();
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        await _store.SaveAsync(ticketEvent);
        SuccessfulBuyers.Add(buyerName);
        return true;
    }
}

public class Program
{
    private const int InitialTickets = 5;
    private const int ConcurrentBuyers = 50;

    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Stage 10 (任務 10.2): 悲觀鎖 vs. 樂觀鎖高併發搶票對照實驗 ===");
        Console.WriteLine($"🎫 每次實驗條件：活動「周杰倫大巨蛋演唱會」，初始票數 {InitialTickets} 張，{ConcurrentBuyers} 位買家同時開搶\n");

        // 實驗 1：無鎖對照組
        var store1 = new InMemoryTicketStore();
        var unsafeService = new UnsafeTicketBookingService(store1);
        await RunExperimentAsync(
            "實驗 1：無併發控制（UnsafeTicketBookingService）",
            store1,
            (eventId, buyer) => unsafeService.TryBookTicketAsync(eventId, buyer),
            () => unsafeService.SuccessfulBuyers.Count,
            todoMessage: null);

        // 實驗 2：悲觀鎖（SemaphoreSlim）
        var store2 = new InMemoryTicketStore();
        var pessimisticService = new PessimisticTicketBookingService(store2);
        await RunExperimentAsync(
            "實驗 2：悲觀鎖（PessimisticTicketBookingService - SemaphoreSlim）",
            store2,
            (eventId, buyer) => pessimisticService.TryBookTicketAsync(eventId, buyer),
            () => pessimisticService.SuccessfulBuyers.Count,
            todoMessage: "⏳ 尚未實作，請完成 ConcurrencyServices.cs 中的 TODO 1 後再執行！");

        // 實驗 3：樂觀鎖（Version + 重試）
        var store3 = new InMemoryTicketStore();
        var optimisticService = new OptimisticTicketBookingService(store3);
        await RunExperimentAsync(
            "實驗 3：樂觀鎖（OptimisticTicketBookingService - Version + 重試）",
            store3,
            (eventId, buyer) => optimisticService.TryBookTicketAsync(eventId, buyer),
            () => optimisticService.SuccessfulBuyers.Count,
            todoMessage: "⏳ 尚未實作，請完成 ConcurrencyServices.cs 中的 TODO 2 後再執行！",
            extraInfoProvider: eventId =>
                $"🔄 版本衝突重試總次數：{optimisticService.ConflictCount} 次 | DB 最終 Version：{store3.PeekVersion(eventId)}");

        // 實驗 4：冪等性扣款（IdempotencyKey 防重試 & 防併發連點）
        await RunIdempotencyExperimentAsync();
    }

    private static async Task RunExperimentAsync(
        string title,
        InMemoryTicketStore store,
        Func<Guid, string, Task<bool>> bookAction,
        Func<int> getSoldCount,
        string? todoMessage,
        Func<Guid, string>? extraInfoProvider = null)
    {
        Console.WriteLine($"--- {title} ---");

        var concert = new TicketEvent("周杰倫大巨蛋演唱會", InitialTickets);
        store.Seed(concert);

        var sw = Stopwatch.StartNew();
        try
        {
            var tasks = Enumerable.Range(1, ConcurrentBuyers)
                .Select(i => bookAction(concert.Id, $"買家-{i:D2}"));

            await Task.WhenAll(tasks);
            sw.Stop();

            int soldCount = getSoldCount();
            int dbRemaining = store.PeekRemainingTickets(concert.Id);
            bool passed = soldCount == InitialTickets && dbRemaining == 0;

            Console.WriteLine($"✅ 實際賣出：{soldCount} 張（預期：{InitialTickets}）| 🗄️ DB 剩餘：{dbRemaining} 張（預期：0）| ⏱️ 耗時：{sw.ElapsedMilliseconds} ms");
            if (extraInfoProvider != null)
            {
                Console.WriteLine(extraInfoProvider(concert.Id));
            }
            Console.WriteLine(passed
                ? "🏆 結果判定：通過！零超賣且庫存完全一致！"
                : $"💥 結果判定：失敗！超賣 {soldCount - InitialTickets} 張或存在幽靈餘票！");
        }
        catch (NotImplementedException)
        {
            sw.Stop();
            Console.WriteLine(todoMessage ?? "⏳ 尚未實作！");
        }

        Console.WriteLine();
    }

    private static async Task RunIdempotencyExperimentAsync()
    {
        Console.WriteLine("--- 實驗 4：冪等性金流扣款（IdempotentPaymentService - IdempotencyKey） ---");

        var gateway = new PaymentGateway();
        var alice = new WalletAccount("Alice", 10_000m);
        gateway.RegisterAccount(alice);
        var paymentService = new IdempotentPaymentService(gateway);

        try
        {
            // 場景 A：模擬網路逾時後，客戶端帶相同 IdempotencyKey 循序重試 3 次
            const string keyA = "pay-order-001";
            var r1 = await paymentService.ChargeAsync(keyA, "Alice", 3_000m);
            var r2 = await paymentService.ChargeAsync(keyA, "Alice", 3_000m);
            var r3 = await paymentService.ChargeAsync(keyA, "Alice", 3_000m);

            bool sameReceiptA = r1.TransactionId == r2.TransactionId && r2.TransactionId == r3.TransactionId;
            bool stateOkA = alice.ChargeCount == 1 && alice.Balance == 7_000m;
            Console.WriteLine($"[場景 A：逾時循序重試 3 次] 扣款次數：{alice.ChargeCount}（預期：1）| 餘額：${alice.Balance}（預期：$7000）| 回傳同一收據：{sameReceiptA}");

            // 場景 B：模擬用戶手抖 / 惡意腳本同一毫秒帶相同 Key 併發送出 10 次扣款請求
            const string keyB = "pay-order-002";
            var concurrentCharges = Enumerable.Range(1, 10)
                .Select(_ => paymentService.ChargeAsync(keyB, "Alice", 2_000m));
            var receiptsB = await Task.WhenAll(concurrentCharges);

            int distinctTxB = receiptsB.Select(r => r.TransactionId).Distinct().Count();
            bool sameReceiptB = distinctTxB == 1 && receiptsB[0].TransactionId != r1.TransactionId;
            bool stateOkB = alice.ChargeCount == 2 && alice.Balance == 5_000m;
            Console.WriteLine($"[場景 B：10 併發瘋狂連點] 總扣款次數：{alice.ChargeCount}（預期：2）| 餘額：${alice.Balance}（預期：$5000）| 不同收據數：{distinctTxB}（預期：1）");

            Console.WriteLine(sameReceiptA && stateOkA && sameReceiptB && stateOkB
                ? "🏆 結果判定：通過！循序重試與併發連點皆做到零重複扣款！"
                : "💥 結果判定：失敗！發生重複扣款或收據不一致！");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("⏳ 尚未實作，請完成 IdempotencyLab.cs 中的 TODO 後再執行！");
        }

        Console.WriteLine();
    }
}
