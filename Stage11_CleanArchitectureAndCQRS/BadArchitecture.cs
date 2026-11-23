namespace Stage11_CleanArchitectureAndCQRS;

// =========================================================================
// ❌ 反模式示範（任務 11.1）：傳統三層架構退化成的「大泥球 (Big Ball of Mud)」
// 請仔細閱讀以下程式碼，診斷它在依賴方向、讀寫耦合與效能擴展上藏了哪些病灶！
// =========================================================================

// ⚠️ 病灶 1：貧血資料模型（ORM 實體）混雜了 DB 技術欄位與 UI / 報表顯示欄位
public class BadOrderItemEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class BadOrderEntity
{
    // --- 領域資料 ---
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public List<BadOrderItemEntity> Items { get; set; } = new();

    // --- DB / ORM 技術欄位 ---
    public bool IsSoftDeleted { get; set; }
    public byte[] DbRowVersion { get; set; } = Array.Empty<byte>();

    // --- 為了偷懶給報表與前端直接輸出，硬塞進來的非領域顯示欄位！ ---
    public string CustomerDisplayNameForUi { get; set; } = string.Empty;
    public string StatusBadgeHtml { get; set; } = string.Empty;
    public int TotalItemsCountForReport { get; set; }
}

// ⚠️ 病灶 2：具體的底層資料庫存取實作 (SQL DAO)
public class BadSqlOrderDao
{
    private readonly string _connectionString;
    private readonly List<BadOrderEntity> _fakeSqlTable = new();

    public BadSqlOrderDao(string connectionString)
    {
        _connectionString = connectionString;
        Console.WriteLine($"[SQL DAO] 連線至資料庫: {_connectionString}");
    }

    public void InsertOrder(BadOrderEntity order)
    {
        Console.WriteLine($"[SQL] INSERT INTO Orders (Id={order.Id}, ...) VALUES (...)");
        _fakeSqlTable.Add(order);
    }

    public void UpdateOrder(BadOrderEntity order)
    {
        Console.WriteLine($"[SQL] UPDATE Orders SET Status='{order.Status}' WHERE Id='{order.Id}'");
    }

    public BadOrderEntity? GetOrderById(Guid id)
    {
        return _fakeSqlTable.FirstOrDefault(o => o.Id == id && !o.IsSoftDeleted);
    }

    public List<BadOrderEntity> GetAllOrdersWithItems()
    {
        // 模擬執行沉重的全表 JOIN，載入所有訂單與底下所有明細物件圖
        Console.WriteLine("[SQL] SELECT * FROM Orders o INNER JOIN OrderItems oi ON o.Id = oi.OrderId WHERE o.IsSoftDeleted = 0");
        return _fakeSqlTable.Where(o => !o.IsSoftDeleted).ToList();
    }
}

// ⚠️ 病灶 3：傳統三層架構的上帝 Service（硬依賴底層 DAO，且寫入與複雜報表查詢混在同一支類別）
public class BadOrderService
{
    private readonly BadSqlOrderDao _dao;

    public BadOrderService()
    {
        // ❌ 高層商業邏輯直接 new 底層具體 SQL DAO，寫死連線字串！
        _dao = new BadSqlOrderDao("Server=prod-sql;Database=ShopDb;Trusted_Connection=True;");
    }

    // --- 寫入流程 (Write / Command) ---
    public Guid PlaceOrder(Guid customerId, string customerName, string productName, decimal price, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("數量必須大於 0");

        var orderId = Guid.NewGuid();
        var order = new BadOrderEntity
        {
            Id = orderId,
            CustomerId = customerId,
            CustomerDisplayNameForUi = customerName,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            IsSoftDeleted = false,
            DbRowVersion = new byte[] { 1, 0, 0, 1 },
            Items = new List<BadOrderItemEntity>
            {
                new BadOrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductName = productName,
                    UnitPrice = price,
                    Quantity = quantity
                }
            }
        };

        _dao.InsertOrder(order);
        return orderId;
    }

    public void CancelOrder(Guid orderId)
    {
        var order = _dao.GetOrderById(orderId)
            ?? throw new InvalidOperationException("找不到訂單");

        // ❌ 貧血實體：在 Service 裡直接修改 public set 屬性
        order.Status = "Cancelled";
        _dao.UpdateOrder(order);
    }

    // --- 複雜報表查詢 (Read / Query) ---
    public List<BadOrderEntity> GetMonthlySalesDashboard()
    {
        // ❌ 為了做管理員儀表板，把全部完整 Order + OrderItem 物件圖撈進記憶體再迴圈算！
        var allOrders = _dao.GetAllOrdersWithItems();

        foreach (var order in allOrders)
        {
            order.TotalItemsCountForReport = order.Items.Sum(i => i.Quantity);
            order.StatusBadgeHtml = order.Status == "Cancelled"
                ? "<span class='badge-gray'>已取消</span>"
                : "<span class='badge-green'>有效訂單</span>";
        }

        return allOrders;
    }
}
