namespace Stage11_CleanArchitectureAndCQRS;

// =========================================================================
// 🏛️ Clean Architecture (六角架構 / Ports & Adapters) 重構實戰
// 核心法則：依賴單向由外向內（Dependency Rule）
// Domain ➔ 完全獨立，零外部相依
// Application ➔ 僅依賴 Domain，定義 Use Cases 與輸出/輸入連接埠 (Ports)
// Infrastructure ➔ 依賴 Application 與 Domain，實作 Ports (Adapters)，隔離 DB/技術細節
// =========================================================================

#region 1. Domain 層 (領域核心 - 零外部相依、充血模型、不變條件守護)

public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}

/// <summary>
/// 領域明細實體 / 聚合內部子實體：絕無 ORM 外鍵、無前端顯示標籤
/// </summary>
public class OrderItem
{
    public Guid Id { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }

    public OrderItem(Guid id, string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("商品名稱不能為空", nameof(productName));
        if (unitPrice < 0)
            throw new ArgumentException("單價不能小於 0", nameof(unitPrice));
        if (quantity <= 0)
            throw new ArgumentException("數量必須大於 0", nameof(quantity));

        Id = id;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal Subtotal => UnitPrice * Quantity;
}

/// <summary>
/// 領域聚合根 (Aggregate Root)：
/// 1. 不含 IsSoftDeleted、DbRowVersion 等資料庫實作細節。
/// 2. 不含 CustomerDisplayNameForUi、StatusBadgeHtml 等前端/報表專屬欄位。
/// 3. Backing Field 徹底封裝明細集合，僅能透過充血方法操作。
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(Guid id, Guid customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // 支援從資料庫還原時的內部建構子
    public Order(Guid id, Guid customerId, OrderStatus status, DateTime createdAt, IEnumerable<OrderItem> items)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
        CreatedAt = createdAt;
        _items.AddRange(items);
    }

    public void AddItem(string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"只有待處理 (Pending) 訂單才能新增明細，當前狀態為: {Status}");

        var item = new OrderItem(Guid.NewGuid(), productName, unitPrice, quantity);
        _items.Add(item);
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("訂單已取消，不可重複取消");

        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("已付款訂單不可直接取消，必須透過退款程序");

        Status = OrderStatus.Cancelled;
    }

    public decimal CalculateTotal() => _items.Sum(i => i.Subtotal);
}

/// <summary>
/// 【Driven Port (被動/輸出埠)】由領域/應用層定義契約，基礎設施層負責實作！
/// 徹底反轉高層依賴底層的傳統架構！
/// </summary>
public interface IOrderRepository
{
    Order? GetById(Guid id);
    void Save(Order order);
}

#endregion

#region 2. Application 層 (應用層 / Use Cases - 協調業務流程，不關心 DB 到底是什麼)

public record PlaceOrderCommand(Guid CustomerId, string ProductName, decimal Price, int Quantity);

/// <summary>
/// 【用例 (Use Case)】下單業務流程：
/// 只和領域聚合根與 Port 介面互動，可在記憶體中極速單元測試！
/// </summary>
public class PlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;

    public PlaceOrderUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Guid Execute(PlaceOrderCommand command)
    {
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, command.CustomerId);
        
        // 充血行為：由聚合根守護驗證
        order.AddItem(command.ProductName, command.Price, command.Quantity);

        // 透過 Port 儲存，Application 完全不知道背後是 SQL Server、PostgreSQL 還是 In-Memory
        _orderRepository.Save(order);

        return orderId;
    }
}

public record CancelOrderCommand(Guid OrderId);

public class CancelOrderUseCase
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public void Execute(CancelOrderCommand command)
    {
        var order = _orderRepository.GetById(command.OrderId)
            ?? throw new InvalidOperationException($"找不到訂單: {command.OrderId}");

        // 呼叫聚合根守門員方法（不變條件保護）
        order.Cancel();

        _orderRepository.Save(order);
    }
}

#endregion

#region 3. Infrastructure 層 (基礎設施層 / Adapters - 實作 Ports，負責與外部系統/DB 溝通)

/// <summary>
/// DB 專屬資料表結構 (ORM 實體)：技術欄位（RowVersion、SoftDelete、FK）完全隔離在此！
/// </summary>
public class OrderDbModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsSoftDeleted { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public List<OrderItemDbModel> Items { get; set; } = new();
}

public class OrderItemDbModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// 【Driven Adapter (次要/驅動適配器)】實作 IOrderRepository：
/// 負責純領域聚合根 (Order) 與 DB Model (OrderDbModel) 之間的雙向 Mapping！
/// </summary>
public class SqlOrderRepositoryAdapter : IOrderRepository
{
    // 模擬實際資料庫表
    private readonly Dictionary<Guid, OrderDbModel> _database = new();

    public Order? GetById(Guid id)
    {
        if (!_database.TryGetValue(id, out var dbModel) || dbModel.IsSoftDeleted)
            return null;

        // DB Model ➔ 純領域聚合根 Mapping
        var domainItems = dbModel.Items.Select(
            i => new OrderItem(i.Id, i.ProductName, i.UnitPrice, i.Quantity));
        var status = Enum.Parse<OrderStatus>(dbModel.Status);

        return new Order(dbModel.Id, dbModel.CustomerId, status, dbModel.CreatedAt, domainItems);
    }

    public void Save(Order order)
    {
        Console.WriteLine($"[Infrastructure/Adapter] 將領域聚合根 (ID={order.Id}) 轉換並持久化至資料庫...");

        // 純領域聚合根 ➔ DB Model Mapping
        var dbModel = new OrderDbModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            IsSoftDeleted = false,
            RowVersion = new byte[] { 1, 0, 0, 2 },
            Items = order.Items.Select(i => new OrderItemDbModel
            {
                Id = i.Id,
                OrderId = order.Id,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        _database[order.Id] = dbModel;
        Console.WriteLine($"[SQL Adapter] 已成功儲存訂單 (狀態: {dbModel.Status}, 明細數: {dbModel.Items.Count})");
    }
}

#endregion
