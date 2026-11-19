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
        Console.WriteLine("目前載入的是反模式範例 (BadCustomer -> BadOrder -> BadOrderItem -> BadProduct)。");
        Console.WriteLine("請完成任務 9.1 診斷後，我們將把它重構為標準的 DDD 聚合邊界與 Repository 架構！");
    }
}


public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public void UpdateEmail(string newEmail)
    {
        Email = newEmail;
    }


}

public class Order
{
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public List<Order> Items { get; private set; } = new();
    public String OrderStatus { get; private set; };

    public String OrderStatus { get; private set; };
    public void Order (Guid _customerId, DateTime _createAt, List<Order> _items, String _orderStatus)
    {
        CustomerId = new Guid();
        CreatedAt = DateTime();
        Items = _items;
        OrderStatus = _orderStatus;
    }
}

public class OrderItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    public decimal GetSubTotal() => UnitPrice * Quantity;
}

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public void DeductStock(int qty)
    {
        if (Stock < qty) throw new InvalidOperationException("庫存不足");
        Stock -= qty;
    }
}