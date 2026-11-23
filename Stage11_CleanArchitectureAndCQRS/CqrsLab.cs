namespace Stage11_CleanArchitectureAndCQRS;

// =========================================================================
// ⚡ CQRS (Command Query Responsibility Segregation) 讀寫職責分離實戰
// =========================================================================
// 核心心法：
// 1. Command (寫入/修改)：重一致性、走充血聚合根、守護不變條件、不回傳肥大實體。
// 2. Query (讀取/報表)：重效能與畫面適配、完全繞過聚合根、直接投影扁平 Read DTO！
// =========================================================================

#region 1. Command Side (寫入端：強一致性、充血聚合根、Invariants 保護)

public record CreateOrderCommand(Guid CustomerId, string CustomerName, string ProductName, decimal Price, int Quantity);

public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public Guid Handle(CreateOrderCommand command)
    {
        // 1. 寫入走完整的充血聚合根
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, command.CustomerId);
        
        // 2. 嚴密業務守門
        order.AddItem(command.ProductName, command.Price, command.Quantity);

        // 3. 透過 Repository 持久化
        _repository.Save(order);

        // 4. Command 原則：只回傳 ID 或成功狀態，不回傳整個實體
        return orderId;
    }
}

#endregion

#region 2. Query Side (讀取端：專屬 Read DTO、繞過領域聚合根、極致效能)

/// <summary>
/// 【專屬 Read DTO】為管理員儀表板 UI 量身打造！
/// 扁平、無行為、包含畫面專屬欄位（如 StatusBadgeHtml、TotalAmount），
/// 徹底避免 UI 欄位污染 Domain 聚合根！
/// </summary>
public record OrderDashboardDto(
    Guid OrderId,
    string CustomerName,
    int TotalItemsCount,
    decimal TotalAmount,
    string StatusBadgeHtml
);

/// <summary>
/// 【查詢介面 (Query Port)】
/// 注意：Query 契約完全獨立於 IOrderRepository，不依賴 Domain 聚合根！
/// </summary>
public interface IOrderQueryService
{
    IReadOnlyList<OrderDashboardDto> GetMonthlySalesDashboard();
}

/// <summary>
/// 【查詢實作 (Query Adapter)】
/// 模擬微 ORM (如 Dapper) 直接下 SQL 投影至 DTO，不經過聚合根，
/// 零物件轉換開銷、零 Over-fetching！
/// </summary>
public class SqlOrderQueryServiceAdapter : IOrderQueryService
{
    private readonly SqlOrderRepositoryAdapter _mockDbStorage;

    // 實務上這裡直接注入 IDbConnection (Dapper) 或唯讀 Read-Replica DbContext
    public SqlOrderQueryServiceAdapter(SqlOrderRepositoryAdapter mockDbStorage)
    {
        _mockDbStorage = mockDbStorage;
    }

    public IReadOnlyList<OrderDashboardDto> GetMonthlySalesDashboard()
    {
        Console.WriteLine("[Query/Dapper] 執行極速 SQL 投影查詢 (SELECT o.Id, c.Name, SUM(...) FROM Orders ... GROUP BY ...)");

        // 模擬直接從 DB 資料表查詢投影成專屬 DTO
        // 在真實場景中：這是一條直接由 SQL 或 Read Replica 產出的扁平資料，
        // 根本不需要將 Orders 與 OrderItems 實例化成幾萬個 C# 物件圖！
        var result = new List<OrderDashboardDto>
        {
            new OrderDashboardDto(
                OrderId: Guid.NewGuid(),
                CustomerName: "Alice (VIP)",
                TotalItemsCount: 2,
                TotalAmount: 6000m,
                StatusBadgeHtml: "<span class='badge-green'>有效訂單</span>"
            ),
            new OrderDashboardDto(
                OrderId: Guid.NewGuid(),
                CustomerName: "Bob",
                TotalItemsCount: 1,
                TotalAmount: 1500m,
                StatusBadgeHtml: "<span class='badge-gray'>已取消</span>"
            )
        };

        return result;
    }
}

#endregion
