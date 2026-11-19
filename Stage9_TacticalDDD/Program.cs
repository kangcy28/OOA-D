namespace Stage9_TacticalDDD;

// =========================================================================
// ❌ 反模式示範：上帝聚合 (God Aggregate) 與過度物件圖耦合
// 請觀察以下這段「看起來很直覺的物件導向設計」，在真實企業系統中會引發什麼災難？
// =========================================================================

public class BadCustomer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // ⚠️ 痛點 1：客戶直接持有「歷史所有訂單」的集合！
    public List<BadOrder> Orders { get; set; } = new();

    public void UpdateEmail(string newEmail)
    {
        Email = newEmail;
    }

    public void PlaceOrder(BadOrder order)
    {
        Orders.Add(order);
    }
}

public class BadOrder
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BadOrderItem> Items { get; set; } = new();
}

public class BadOrderItem
{
    // ⚠️ 痛點 2：訂單明細直接持有整個 Product 實體物件！
    public BadProduct Product { get; set; } = null!;
    public int Quantity { get; set; }

    public decimal GetSubTotal() => Product.Price * Quantity;
}

public class BadProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public void DeductStock(int qty)
    {
        if (Stock < qty) throw new InvalidOperationException("庫存不足");
        Stock -= qty;
    }
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Stage 9 (任務 9.3): 持久化解耦與 Unit of Work 交易驗證 ===");

        // 1. 組裝基礎設施層與應用層服務
        var db = new InMemoryDatabase();
        var unitOfWork = new InMemoryUnitOfWork();
        IOrderRepository orderRepo = new InMemoryOrderRepository(db, unitOfWork);
        IProductRepository productRepo = new InMemoryProductRepository(db, unitOfWork);
        var placeOrderService = new PlaceOrderService(orderRepo, productRepo, unitOfWork);

        // 2. 初始化測試資料（商品與客戶）
        var keyboard = new Product("機械鍵盤", 3000m, 10);
        db.Products[keyboard.Id] = keyboard.Clone();
        var customer = new Customer("Alice", "alice@example.com");

        Console.WriteLine($"[初始狀態] 商品: {keyboard.Name}, 單價: ${keyboard.Price}, 初始庫存: {db.Products[keyboard.Id].Stock}\n");

        // 3. 場景 A：正常下單（購買 2 個鍵盤）
        Console.WriteLine("--- 場景 A：正常下單 (購買 2 個) ---");
        var orderId = placeOrderService.PlaceOrder(customer.Id, keyboard.Id, 2);
        var savedOrder = db.Orders[orderId];
        Console.WriteLine($"✅ 下單成功！訂單 ID: {savedOrder.Id}");
        Console.WriteLine($"   訂單總金額: ${savedOrder.TotalAmount} (預期: $6000)");
        Console.WriteLine($"   DB 剩餘庫存: {db.Products[keyboard.Id].Stock} (預期: 8)\n");

        // 4. 場景 B：中途失敗回滾（扣完庫存登記後，建立 Order 時因 customerId 為 Guid.Empty 觸發例外）
        Console.WriteLine("--- 場景 B：下單中途失敗，驗證交易回滾 ---");
        try
        {
            placeOrderService.PlaceOrder(Guid.Empty, keyboard.Id, 3);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ 捕獲預期例外: {ex.Message}");
        }

        Console.WriteLine($"🛡️ 回滾後 DB 訂單總筆數: {db.Orders.Count} (預期: 1)");
        Console.WriteLine($"🛡️ 回滾後 DB 剩餘庫存: {db.Products[keyboard.Id].Stock} (預期依然維持: 8)");
    }
}


public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public Customer(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
    }

    public void UpdateEmail(string newEmail)
    {
        Email = newEmail;
    }
}

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string OrderStatus { get; private set; }

    // 🔒 經典 Backing Field（背後欄位）：內部可控，對外只暴露唯讀集合
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.GetSubTotal());

    public Order(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("客戶 ID 不可為空！", nameof(customerId));

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        OrderStatus = "Pending";
    }

    // 🛡️ 聚合根守門員：統一守護 Invariants（不變條件）
    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (OrderStatus != "Pending")
            throw new InvalidOperationException("只有待處理的訂單才能新增商品");

        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
    }
}

public class OrderItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public decimal GetSubTotal() => UnitPrice * Quantity;
}

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public Product(string name, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        Stock = stock;
    }

    // 🔒 私有還原建構子：供持久化層從資料重建聚合根（保留原本 ID）
    private Product(Guid id, string name, decimal price, int stock)
    {
        Id = id;
        Name = name;
        Price = price;
        Stock = stock;
    }

    // 🔄 模擬資料庫讀取時的反序列化複本，避免未 Commit 前直接污染 DB 記憶體參考
    internal Product Clone() => new Product(Id, Name, Price, Stock);

    public void DeductStock(int qty)
    {
        if (qty <= 0) throw new ArgumentException("扣減數量必須大於 0", nameof(qty));
        if (Stock < qty) throw new InvalidOperationException("庫存不足");
        Stock -= qty;
    }
}