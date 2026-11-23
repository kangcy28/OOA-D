namespace Stage11_CleanArchitectureAndCQRS;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Stage 11 (任務 11.1): 傳統三層架構「大泥球」病灶診斷 ===");

        var service = new BadOrderService();

        // 1. 模擬寫入兩筆範例訂單
        var order1Id = service.PlaceOrder(Guid.NewGuid(), "Alice", "機械鍵盤", 3000m, 2);
        var order2Id = service.PlaceOrder(Guid.NewGuid(), "Bob", "無線滑鼠", 1500m, 1);
        service.CancelOrder(order2Id);

        Console.WriteLine($"\n已建立訂單: {order1Id}, {order2Id} (其中第 2 筆已取消)\n");

        // 2. 模擬複雜報表查詢（讀寫共用同一實體與 Service）
        var dashboardOrders = service.GetMonthlySalesDashboard();

        Console.WriteLine("\n--- 管理員月銷售儀表板輸出 ---");
        foreach (var order in dashboardOrders)
        {
            var total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            Console.WriteLine(
                $"客戶: {order.CustomerDisplayNameForUi} | 數量: {order.TotalItemsCountForReport} | 金額: ${total} | 狀態標籤: {order.StatusBadgeHtml}");
        }

        Console.WriteLine("\n💡 請先閱讀 BadArchitecture.cs 完成「任務 11.1 病灶診斷」！");
        Console.WriteLine("   通關後我們將於【任務 11.2】導入 Clean Architecture 依賴反轉，並於【任務 11.3】導入 CQRS 讀寫分離！");
    }
}
