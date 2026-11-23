namespace Stage11_CleanArchitectureAndCQRS;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=========================================================================");
        Console.WriteLine(" ⚡ Stage 11 (任務 11.3): CQRS 讀寫職責分離實戰");
        Console.WriteLine("=========================================================================\n");

        // 1. 初始化基礎設施 (Adapters)
        var sqlRepoAdapter = new SqlOrderRepositoryAdapter();
        IOrderRepository repository = sqlRepoAdapter;
        IOrderQueryService queryService = new SqlOrderQueryServiceAdapter(sqlRepoAdapter);

        // 2. 初始化 Command Handlers (寫入端)
        var createOrderHandler = new CreateOrderCommandHandler(repository);
        var cancelOrderUseCase = new CancelOrderUseCase(repository);

        // --- 【Command Side 實戰：狀態修改走聚合根】 ---
        Console.WriteLine(">>> [Command Side] 1. 發送 CreateOrderCommand (寫入端)...");
        var orderId1 = createOrderHandler.Handle(new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            CustomerName: "Alice",
            ProductName: "4K 電競螢幕",
            Price: 12000m,
            Quantity: 1
        ));
        Console.WriteLine($"✅ Command 執行完畢，成功建立訂單 ID: {orderId1}\n");

        Console.WriteLine(">>> [Command Side] 2. 發送 CancelOrderCommand (狀態流轉)...");
        cancelOrderUseCase.Execute(new CancelOrderCommand(orderId1));
        Console.WriteLine($"✅ Command 執行完畢，訂單狀態已轉移。\n");

        // --- 【Query Side 實戰：報表查詢直出輕量 Read DTO】 ---
        Console.WriteLine(">>> [Query Side] 3. 執行 GetMonthlySalesDashboard (讀取端)...");
        var dashboardData = queryService.GetMonthlySalesDashboard();

        Console.WriteLine("\n📊 【管理員儀表板輸出 (Read DTO 直出，零領域聚合根開銷)】");
        foreach (var dto in dashboardData)
        {
            Console.WriteLine(
                $"  - 訂單: {dto.OrderId.ToString()[..8]}... | 客戶: {dto.CustomerName,-12} | 品項數: {dto.TotalItemsCount} | 總額: ${dto.TotalAmount,7} | 標籤: {dto.StatusBadgeHtml}");
        }

        Console.WriteLine("\n=========================================================================");
        Console.WriteLine("🎉 CQRS 讀寫分離核心價值對比：");
        Console.WriteLine(" 寫入 (Command) : 堅持走充血聚合根，嚴格校驗業務不變條件，只回傳 ID / 狀態。");
        Console.WriteLine(" 讀取 (Query)   : 完全繞過聚合根，SQL/Dapper 直接投影到專屬 Read DTO，");
        Console.WriteLine("                  杜絕 Over-fetching，領域模型與 UI 需求徹底解耦！");
        Console.WriteLine("=========================================================================");
    }
}
