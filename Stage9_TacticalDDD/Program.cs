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
        Console.WriteLine("=== Stage 9: 戰術 DDD 聚合邊界實戰 ===");
        
        // 1. 建立商品與客戶（獨立聚合，各自管理生命週期）
        var product = new Product("機械鍵盤", 3000m, 10);
        var customer = new Customer("Alice", "alice@example.com");

        // 2. 建立訂單聚合根（跨聚合僅以 ID 參照，不直接抓取整個 Customer 實體）
        var order = new Order(customer.Id);
        
        // 3. 聚合根統一守門：由 Order 守護不變條件，下單當下拍下名稱與價格快照
        order.AddItem(product.Id, product.Name, product.Price, 2);
        product.DeductStock(2);

        Console.WriteLine($"[訂單建立成功]");
        Console.WriteLine($"訂單 ID: {order.Id}");
        Console.WriteLine($"客戶 ID: {order.CustomerId}");
        Console.WriteLine($"訂單狀態: {order.OrderStatus}");
        Console.WriteLine($"訂單項目數: {order.Items.Count}");
        Console.WriteLine($"訂單總金額: ${order.TotalAmount}");

        // 4. 驗證防價格漂移：商品後續漲價，不影響已建立訂單的歷史快照
        product.Price = 5000m;
        Console.WriteLine($"\n[商品漲價為 ${product.Price}]");
        Console.WriteLine($"訂單歷史單價維持: ${order.Items.First().UnitPrice}");
        Console.WriteLine($"訂單總額依然為: ${order.TotalAmount} (完全不受影響！)");
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

    public void DeductStock(int qty)
    {
        if (Stock < qty) throw new InvalidOperationException("庫存不足");
        Stock -= qty;
    }
}