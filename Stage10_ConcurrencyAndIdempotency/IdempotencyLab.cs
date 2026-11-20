using System.Collections.Concurrent;

namespace Stage10_ConcurrencyAndIdempotency;

/// <summary>
/// 扣款成功後的交易收據（不可變 Value Object / DTO）。
/// </summary>
public record PaymentReceipt(
    Guid TransactionId,
    string IdempotencyKey,
    string UserId,
    decimal Amount,
    DateTime ProcessedAt);

/// <summary>
/// 用戶錢包帳戶：記錄餘額與實際被扣款次數（用於驗證是否重複扣款）。
/// </summary>
public class WalletAccount
{
    private readonly object _lock = new();

    public string UserId { get; }
    public decimal Balance { get; private set; }
    public int ChargeCount { get; private set; }

    public WalletAccount(string userId, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId 不可為空", nameof(userId));
        if (initialBalance < 0)
            throw new ArgumentException("初始餘額不可為負數", nameof(initialBalance));

        UserId = userId;
        Balance = initialBalance;
    }

    public void Deduct(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("扣款金額必須大於 0", nameof(amount));

        lock (_lock)
        {
            if (Balance < amount)
                throw new InvalidOperationException("餘額不足");

            Balance -= amount;
            ChargeCount++;
        }
    }
}

/// <summary>
/// 模擬真實金流閘道器：每次呼叫皆會有 50ms 網路延遲，並直接對帳戶進行扣款。
/// 若上層未做好冪等性防護，重複送出就會重複扣款！
/// </summary>
public class PaymentGateway
{
    private readonly ConcurrentDictionary<string, WalletAccount> _accounts = new();

    public void RegisterAccount(WalletAccount account) =>
        _accounts[account.UserId] = account;

    public WalletAccount GetAccount(string userId) => _accounts[userId];

    public async Task<PaymentReceipt> ChargeAsync(string idempotencyKey, string userId, decimal amount)
    {
        // 模擬呼叫第三方金流 API 的網路 I/O 延遲（放大併發連點的重疊視窗）
        await Task.Delay(50);

        _accounts[userId].Deduct(amount);
        return new PaymentReceipt(Guid.NewGuid(), idempotencyKey, userId, amount, DateTime.UtcNow);
    }
}

/// <summary>
/// 冪等性付款服務（Idempotency-Key + Per-Key 細粒度鎖 + Double-Check 防穿透）。
/// </summary>
public class IdempotentPaymentService
{
    private readonly PaymentGateway _gateway;
    private readonly ConcurrentDictionary<string, PaymentReceipt> _processedReceipts = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    public IdempotentPaymentService(PaymentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<PaymentReceipt> ChargeAsync(string idempotencyKey, string userId, decimal amount)
    {
        // TODO：請實作 IdempotencyKey 防重複扣款邏輯（同時抵擋循序重試與併發連點）
        // 實作提示：
        // 1. 防呆檢查：if (string.IsNullOrWhiteSpace(idempotencyKey))
        //    throw new ArgumentException("IdempotencyKey 不可為空", nameof(idempotencyKey));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("IdempotencyKey 不可為空", nameof(idempotencyKey));
        // 2. Fast Path（無鎖快取查詢）：
        //    若 _processedReceipts.TryGetValue(idempotencyKey, out var cachedReceipt) 為 true，直接 return cachedReceipt;
        if (_processedReceipts.TryGetValue(idempotencyKey, out var cachedReceipt)) return cachedReceipt;
        // 3. 取得該 Key 專屬的細粒度鎖（避免鎖住其他無關用戶的付款請求）：
        //    var keyLock = _keyLocks.GetOrAdd(idempotencyKey, _ => new SemaphoreSlim(1, 1));
        var keyLock = _keyLocks.GetOrAdd(idempotencyKey, _ => new SemaphoreSlim(1, 1));
        // 4. 進入鎖：await keyLock.WaitAsync(); 並用 try { ... } finally { keyLock.Release(); } 確保必定放鎖
        await keyLock.WaitAsync();
        try
        {
           if (_processedReceipts.TryGetValue(idempotencyKey, out cachedReceipt)) return cachedReceipt;
           var receipt = await _gateway.ChargeAsync(idempotencyKey, userId, amount);
           _processedReceipts[idempotencyKey] = receipt;
           return receipt;
        }
        finally 
        { 
            keyLock.Release(); 
        }
        // 5. Double-Check（關鍵防穿透）：
        //    進鎖後務必再檢查一次 _processedReceipts.TryGetValue(idempotencyKey, out cachedReceipt)！
        //    若前一個併發進來的同 Key 請求已經完成扣款並寫入快取，這裡就直接 return cachedReceipt;
        // 6. 實際執行扣款：var receipt = await _gateway.ChargeAsync(idempotencyKey, userId, amount);
        // 7. 寫入冪等快取：_processedReceipts[idempotencyKey] = receipt;
        // 8. return receipt;
    }
}
