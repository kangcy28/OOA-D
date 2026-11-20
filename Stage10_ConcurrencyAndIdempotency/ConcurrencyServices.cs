using System.Collections.Concurrent;

namespace Stage10_ConcurrencyAndIdempotency;

/// <summary>
/// 方案 A：悲觀鎖（Pessimistic Locking）—— 先搶鎖、進門檢查扣票寫回、最後放鎖。
/// </summary>
public class PessimisticTicketBookingService
{
    private readonly InMemoryTicketStore _store;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ConcurrentBag<string> SuccessfulBuyers { get; } = new();

    public PessimisticTicketBookingService(InMemoryTicketStore store)
    {
        _store = store;
    }

    public async Task<bool> TryBookTicketAsync(Guid eventId, string buyerName)
    {
        await _semaphore.WaitAsync();
        try
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
        finally
        {
            _semaphore.Release();
        }
    }
}

/// <summary>
/// 方案 B：樂觀鎖（Optimistic Locking）—— 不搶鎖直接讀與改，寫回時比對 Version，衝突則重讀重試。
/// </summary>
public class OptimisticTicketBookingService
{
    private readonly InMemoryTicketStore _store;
    private int _conflictCount = 0;

    public int ConflictCount => _conflictCount;
    public ConcurrentBag<string> SuccessfulBuyers { get; } = new();

    public OptimisticTicketBookingService(InMemoryTicketStore store)
    {
        _store = store;
    }

    public async Task<bool> TryBookTicketAsync(Guid eventId, string buyerName, int maxRetries = 20)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var ticketEvent = await _store.GetByIdAsync(eventId);
            if (ticketEvent == null) return false;

            int expectedVersion = ticketEvent.Version;
            try
            {
                ticketEvent.ReserveOne();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            bool saved = await _store.TrySaveWithVersionAsync(ticketEvent, expectedVersion);
            if (saved)
            {
                SuccessfulBuyers.Add(buyerName);
                return true;
            }

            Interlocked.Increment(ref _conflictCount);
        }

        return false;
    }
}
