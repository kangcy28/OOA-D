namespace Stage12_DistributedConsistency;

// =========================================================================
// 🔄 任務 12.3：Saga 模式與補償交易 (Orchestration Saga & Compensating Transactions)
// 核心概念：
// 在微服務架構下，每個服務擁有獨立資料庫，無法使用單一本地 ACID Transaction 跨服務回滾。
// Saga 將一個長流程拆成多個本地交易（T1 -> T2 -> T3），每個動作都配有對應的
// 「語意補償交易（Compensating Transaction: C1, C2）」。
// 當第 K 步失敗時，協調器（Orchestrator）按「後進先出 (LIFO) 逆序」觸發補償，
// 讓跨服務系統最終回到商業語意上的一致狀態！
// =========================================================================

public enum SagaOrderStatus
{
    Pending,
    Confirmed,
    Cancelled
}

/// <summary>
/// 訂單微服務中的訂單領域模型：採用語意回滾（狀態轉為 Cancelled 並保留稽核原因），而非實體 DELETE。
/// </summary>
public sealed class SagaOrder
{
    public Guid Id { get; }
    public string CustomerName { get; }
    public string ProductName { get; }
    public decimal Amount { get; }
    public SagaOrderStatus Status { get; private set; }
    public string? CancelReason { get; private set; }

    public SagaOrder(Guid id, string customerName, string productName, decimal amount)
    {
        Id = id;
        CustomerName = customerName;
        ProductName = productName;
        Amount = amount;
        Status = SagaOrderStatus.Pending;
    }

    public void Confirm()
    {
        if (Status == SagaOrderStatus.Cancelled)
            throw new InvalidOperationException("已取消的訂單無法確認！");

        Status = SagaOrderStatus.Confirmed;
    }

    /// <summary>
    /// 冪等補償操作：重複呼叫取消也不會重複產生副作用或拋錯。
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == SagaOrderStatus.Cancelled)
            return;

        Status = SagaOrderStatus.Cancelled;
        CancelReason = reason;
    }
}

/// <summary>
/// 獨立微服務 1：訂單服務（擁有獨立訂單儲存庫）。
/// </summary>
public sealed class SagaOrderService
{
    private readonly Dictionary<Guid, SagaOrder> _orders = new();

    public Guid CreatePendingOrder(string customerName, string productName, decimal amount)
    {
        var order = new SagaOrder(Guid.NewGuid(), customerName, productName, amount);
        _orders[order.Id] = order;
        Console.WriteLine($"[OrderService] 📝 T1 建立待處理訂單 (OrderId={order.Id.ToString()[..8]}..., Status={order.Status})");
        return order.Id;
    }

    public void ConfirmOrder(Guid orderId)
    {
        var order = _orders[orderId];
        order.Confirm();
        Console.WriteLine($"[OrderService] ✅ T4 訂單已確認成立 (OrderId={order.Id.ToString()[..8]}..., Status={order.Status})");
    }

    /// <summary>
    /// C1 補償交易：語意取消訂單（保留稽核紀錄而非 DELETE）。
    /// </summary>
    public void CancelOrder(Guid orderId, string reason)
    {
        if (!_orders.TryGetValue(orderId, out var order))
            return;

        if (order.Status == SagaOrderStatus.Cancelled)
        {
            Console.WriteLine($"[OrderService] 🛡️ C1 訂單 ({orderId.ToString()[..8]}...) 已為 Cancelled，冪等略過。");
            return;
        }

        order.Cancel(reason);
        Console.WriteLine($"[OrderService] ↩️ C1 補償執行：訂單狀態轉為 {order.Status}（原因：{reason}）");
    }

    public SagaOrder? GetOrder(Guid orderId) =>
        _orders.TryGetValue(orderId, out var order) ? order : null;
}

/// <summary>
/// 獨立微服務 2：庫存服務（擁有獨立庫存儲存庫）。
/// </summary>
public sealed class SagaInventoryService
{
    private readonly Dictionary<string, int> _stocks = new();
    private readonly HashSet<Guid> _reservedOrders = new();

    public void SeedStock(string productName, int quantity)
    {
        _stocks[productName] = quantity;
    }

    public int GetStock(string productName) =>
        _stocks.TryGetValue(productName, out var qty) ? qty : 0;

    public void ReserveStock(Guid orderId, string productName, int quantity)
    {
        int current = GetStock(productName);
        if (current < quantity)
        {
            throw new InvalidOperationException($"商品「{productName}」庫存不足（剩餘 {current}，需求 {quantity}）！");
        }

        _stocks[productName] = current - quantity;
        _reservedOrders.Add(orderId);
        Console.WriteLine($"[InventoryService] 📦 T2 預扣庫存成功：「{productName}」-{quantity}（剩餘：{_stocks[productName]}）");
    }

    /// <summary>
    /// C2 補償交易：釋放預扣庫存（必須具備冪等性，避免重試時多加庫存）。
    /// </summary>
    public void ReleaseStock(Guid orderId, string productName, int quantity)
    {
        if (!_reservedOrders.Remove(orderId))
        {
            Console.WriteLine($"[InventoryService] 🛡️ C2 訂單 ({orderId.ToString()[..8]}...) 庫存已釋放或未預扣，冪等略過。");
            return;
        }

        _stocks[productName] = GetStock(productName) + quantity;
        Console.WriteLine($"[InventoryService] ↩️ C2 補償執行：歸還「{productName}」+{quantity}（恢復為：{_stocks[productName]}）");
    }
}

/// <summary>
/// 獨立微服務 3：金流服務（關鍵轉折點 Pivot Transaction）。
/// </summary>
public sealed class SagaPaymentService
{
    public bool ShouldFail { get; set; }
    public int SuccessfulChargeCount { get; private set; }

    public void Charge(Guid orderId, string customerName, decimal amount)
    {
        if (ShouldFail)
        {
            Console.WriteLine($"[PaymentService] ❌ T3 信用卡扣款失敗！OrderId={orderId.ToString()[..8]}..., 金額=${amount}");
            throw new InvalidOperationException("信用卡扣款失敗（餘額不足或風控拒絕）！");
        }

        SuccessfulChargeCount++;
        Console.WriteLine($"[PaymentService] 💳 T3 扣款成功：{customerName} 已支付 ${amount}（OrderId={orderId.ToString()[..8]}...）");
    }
}

public sealed record SagaExecutionResult(bool IsSuccess, Guid? OrderId, string Message);

/// <summary>
/// 編排式 Saga 協調器 (Orchestrator)：
/// 集中控制跨服務交易步驟，並維護 LIFO 補償堆疊，確保任一步驟失敗時依逆序補償已成功的前置步驟。
/// </summary>
public sealed class OrderCheckoutSagaOrchestrator
{
    private readonly SagaOrderService _orderService;
    private readonly SagaInventoryService _inventoryService;
    private readonly SagaPaymentService _paymentService;

    public OrderCheckoutSagaOrchestrator(
        SagaOrderService orderService,
        SagaInventoryService inventoryService,
        SagaPaymentService paymentService)
    {
        _orderService = orderService;
        _inventoryService = inventoryService;
        _paymentService = paymentService;
    }

    public SagaExecutionResult ExecuteCheckout(string customerName, string productName, int quantity, decimal amount)
    {
        var compensations = new Stack<(string Name, Action<string> Execute)>();
        Guid? createdOrderId = null;

        try
        {
            // Step 1 (T1): 建立 Pending 訂單
            Guid orderId = _orderService.CreatePendingOrder(customerName, productName, amount);
            createdOrderId = orderId;
            compensations.Push(("取消訂單 CancelOrder", reason => _orderService.CancelOrder(orderId, reason)));

            // Step 2 (T2): 預扣庫存
            _inventoryService.ReserveStock(orderId, productName, quantity);
            compensations.Push(("釋放庫存 ReleaseStock", _ => _inventoryService.ReleaseStock(orderId, productName, quantity)));

            // Step 3 (T3 / Pivot Transaction): 金流扣款
            _paymentService.Charge(orderId, customerName, amount);

            // Step 4 (T4): 確認訂單成立
            _orderService.ConfirmOrder(orderId);

            return new SagaExecutionResult(true, orderId, "結帳流程全部成功完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SagaOrchestrator] 🚨 流程於中途失敗：{ex.Message}");
            Console.WriteLine("[SagaOrchestrator] 🔄 開始依 LIFO 逆序執行補償交易（Compensating Transactions）...");

            while (compensations.Count > 0)
            {
                var (name, execute) = compensations.Pop();
                Console.WriteLine($"[SagaOrchestrator] ➔ 觸發補償：{name}");
                execute(ex.Message);
            }

            return new SagaExecutionResult(false, createdOrderId, ex.Message);
        }
    }
}
