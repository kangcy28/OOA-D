namespace Stage12_DistributedConsistency;

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=========================================================================");
        Console.WriteLine(" 💥 Stage 12 (任務 12.1): 分散式「雙寫危機 (Dual-Write Problem)」重現實驗");
        Console.WriteLine("=========================================================================\n");

        RunExperimentA_DbFirst();
        RunExperimentB_MqFirst();
        RunExperimentC_PublishInsideTransaction();

        Console.WriteLine("=========================================================================");
        Console.WriteLine(" 🛡️ Stage 12 (任務 12.2): Transactional Outbox Pattern 實戰驗證");
        Console.WriteLine("=========================================================================\n");

        RunTask12_2_OutboxVerification();

        Console.WriteLine("\n=========================================================================");
        Console.WriteLine(" 🔄 Stage 12 (任務 12.3): Saga 編排式補償交易實戰驗證");
        Console.WriteLine("=========================================================================\n");

        RunTask12_3_SagaDemo();
    }

    private static void RunExperimentA_DbFirst()
    {
        Console.WriteLine("--- 實驗 A：先寫 DB 並 Commit，再發送 RabbitMQ（模擬 MQ 瞬斷） ---");
        var db = new SimulatedOrderDatabase();
        var broker = new SimulatedRabbitMqBroker { IsDown = true };
        var service = new DbFirstOrderService(db, broker);

        try
        {
            service.PlaceOrder("Alice", "機械鍵盤", 3500m);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Catch] 捕捉到異常：{ex.Message}");
        }

        PrintAudit("實驗 A", db, broker, "💥 幽靈孤兒訂單：顧客已下單，但庫存沒扣、物流沒收到通知！");
    }

    private static void RunExperimentB_MqFirst()
    {
        Console.WriteLine("--- 實驗 B：先發送 RabbitMQ，再寫 DB（模擬 DB Commit 失敗） ---");
        var db = new SimulatedOrderDatabase { FailOnCommit = true };
        var broker = new SimulatedRabbitMqBroker();
        var service = new MqFirstOrderService(db, broker);

        try
        {
            service.PlaceOrder("Bob", "電競滑鼠", 1800m);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Catch] 捕捉到異常：{ex.Message}");
        }

        PrintAudit("實驗 B", db, broker, "💥 幽靈事件：DB 裡根本沒這筆訂單，下游卻已扣庫存並安排出貨！");
    }

    private static void RunExperimentC_PublishInsideTransaction()
    {
        Console.WriteLine("--- 實驗 C：把發 MQ 塞進 DB Transaction 裡（模擬 Commit 死鎖失敗） ---");
        var db = new SimulatedOrderDatabase { FailOnCommit = true };
        var broker = new SimulatedRabbitMqBroker();
        var service = new PublishInsideTransactionOrderService(db, broker);

        try
        {
            service.PlaceOrder("Charlie", "4K 螢幕", 12000m);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Catch] 捕捉到異常：{ex.Message}");
        }

        PrintAudit("實驗 C", db, broker, "💥 外部 MQ 不受本地 DB Rollback 管轄，且網路 I/O 拉長了 DB 交易鎖時間！");
    }

    private static void PrintAudit(
        string experimentName,
        SimulatedOrderDatabase db,
        SimulatedRabbitMqBroker broker,
        string verdict)
    {
        Console.WriteLine($"📊 [{experimentName} 稽核結果] DB 訂單數 = {db.CommittedOrders.Count} | MQ 事件數 = {broker.PublishedEvents.Count}");
        Console.WriteLine($"   {verdict}\n");
    }

    private static void RunTask12_2_OutboxVerification()
    {
        try
        {
            bool exp1 = RunOutboxExperiment1_DbCommitFail();
            bool exp2 = RunOutboxExperiment2_MqDownThenRecover();
            bool exp3 = RunOutboxExperiment3_WorkerCrashAtLeastOnceAndIdempotency();

            Console.WriteLine("=========================================================================");
            if (exp1 && exp2 && exp3)
            {
                Console.WriteLine("🏆 任務 12.2 全部通過！成功以 Transactional Outbox + 冪等消費者消滅雙寫危機！");
            }
            else
            {
                Console.WriteLine("⚠️ 部分 Outbox 實驗未通過，請檢查 OutboxLab.cs 中的實作邏輯！");
            }
            Console.WriteLine("=========================================================================");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("⏳ 尚未實作，請完成 OutboxLab.cs 中的 TODO 1 ~ TODO 3 後再執行！");
        }
    }

    private static bool RunOutboxExperiment1_DbCommitFail()
    {
        Console.WriteLine("--- Outbox 實驗 1：DB Commit 失敗 ➔ 訂單與 Outbox 同生共死（零幽靈事件） ---");
        var db = new OutboxOrderDatabase { FailOnCommit = true };
        var broker = new SimulatedRabbitMqBroker();
        var orderService = new OutboxOrderService(db);
        var relayWorker = new OutboxRelayWorker(db, broker);

        try
        {
            orderService.PlaceOrder("Alice", "機械鍵盤", 3500m);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[Catch] 預期捕捉到 DB 異常：{ex.Message}");
        }

        relayWorker.ProcessPendingMessagesOnce();

        bool passed = db.CommittedOrders.Count == 0
            && db.CommittedOutboxMessages.Count == 0
            && broker.PublishedEvents.Count == 0;

        Console.WriteLine($"📊 [Outbox 實驗 1 稽核] DB 訂單數 = {db.CommittedOrders.Count} | Outbox 數 = {db.CommittedOutboxMessages.Count} | MQ 事件數 = {broker.PublishedEvents.Count}");
        Console.WriteLine(passed ? "✅ 通過：DB 失敗時不會留下任何孤兒訂單或幽靈事件！\n" : "❌ 失敗：發生殘留資料或幽靈事件！\n");
        return passed;
    }

    private static bool RunOutboxExperiment2_MqDownThenRecover()
    {
        Console.WriteLine("--- Outbox 實驗 2：RabbitMQ 瞬斷 ➔ 下單照常秒成功，MQ 恢復後自動補發 ---");
        var db = new OutboxOrderDatabase();
        var broker = new SimulatedRabbitMqBroker { IsDown = true };
        var consumer = new IdempotentInventoryConsumer();
        broker.OnEventPublished += consumer.Handle;

        var orderService = new OutboxOrderService(db);
        var relayWorker = new OutboxRelayWorker(db, broker);

        orderService.PlaceOrder("Bob", "電競滑鼠", 1800m);

        Console.WriteLine(">>> [第 1 輪巡檢] 此時 RabbitMQ 處於離線狀態...");
        relayWorker.ProcessPendingMessagesOnce();

        bool round1Ok = db.CommittedOutboxMessages.Count == 1
            && db.CommittedOutboxMessages[0].Status == OutboxStatus.Pending
            && db.CommittedOutboxMessages[0].RetryCount == 1
            && broker.PublishedEvents.Count == 0;

        Console.WriteLine(">>> [第 2 輪巡檢] RabbitMQ 恢復上線！");
        broker.IsDown = false;
        relayWorker.ProcessPendingMessagesOnce();

        bool round2Ok = db.CommittedOutboxMessages[0].Status == OutboxStatus.Processed
            && broker.PublishedEvents.Count == 1
            && consumer.StockDeductedCount == 1;

        bool passed = round1Ok && round2Ok;
        Console.WriteLine($"📊 [Outbox 實驗 2 稽核] Outbox 狀態 = {db.CommittedOutboxMessages[0].Status} | RetryCount = {db.CommittedOutboxMessages[0].RetryCount} | MQ 事件數 = {broker.PublishedEvents.Count} | 扣庫存次數 = {consumer.StockDeductedCount}");
        Console.WriteLine(passed ? "✅ 通過：下單與 MQ 解耦且恢復後自動補齊投遞！\n" : "❌ 失敗：MQ 瞬斷重試或補發結果不符合預期！\n");
        return passed;
    }

    private static bool RunOutboxExperiment3_WorkerCrashAtLeastOnceAndIdempotency()
    {
        Console.WriteLine("--- Outbox 實驗 3：發完 MQ 後 Worker 崩潰未標記 Processed ➔ 重送 + 冪等消費者去重 ---");
        var db = new OutboxOrderDatabase();
        var broker = new SimulatedRabbitMqBroker();
        var consumer = new IdempotentInventoryConsumer();
        broker.OnEventPublished += consumer.Handle;

        var orderService = new OutboxOrderService(db);
        var relayWorker = new OutboxRelayWorker(db, broker);

        orderService.PlaceOrder("Charlie", "4K 螢幕", 12000m);

        Console.WriteLine(">>> [第 1 輪巡檢] 模擬 MQ 投遞成功，但寫回 Processed 之前 Worker 突然崩潰！");
        db.FailOnMarkProcessed = true;
        relayWorker.ProcessPendingMessagesOnce();

        bool round1Ok = db.CommittedOutboxMessages.Count == 1
            && db.CommittedOutboxMessages[0].Status == OutboxStatus.Pending
            && db.CommittedOutboxMessages[0].RetryCount == 1
            && broker.PublishedEvents.Count == 1
            && consumer.StockDeductedCount == 1;

        Console.WriteLine(">>> [第 2 輪巡檢] Worker 重啟恢復，重新投遞同一個 EventId 並完成標記！");
        db.FailOnMarkProcessed = false;
        relayWorker.ProcessPendingMessagesOnce();

        bool round2Ok = broker.PublishedEvents.Count == 2
            && consumer.StockDeductedCount == 1
            && consumer.DuplicateIgnoredCount == 1
            && db.CommittedOutboxMessages.All(m => m.Status == OutboxStatus.Processed);

        bool passed = round1Ok && round2Ok;
        Console.WriteLine($"📊 [Outbox 實驗 3 稽核] MQ 總投遞 = {broker.PublishedEvents.Count} 次 | 實際扣庫存 = {consumer.StockDeductedCount} 次 | 攔截重複事件 = {consumer.DuplicateIgnoredCount} 次 | Outbox 狀態 = {db.CommittedOutboxMessages[0].Status}");
        Console.WriteLine(passed ? "✅ 通過：At-Least-Once 重送搭配冪等消費者實現 Effectively-Once 效果！\n" : "❌ 失敗：重複投遞或冪等去重結果不符合預期！\n");
        return passed;
    }

    private static void RunTask12_3_SagaDemo()
    {
        bool exp1 = RunSagaExperiment1_HappyPath();
        bool exp2 = RunSagaExperiment2_PaymentFailureCompensation();

        Console.WriteLine("=========================================================================");
        if (exp1 && exp2)
        {
            Console.WriteLine("🏆 任務 12.3 Saga 示範實驗全部通過！正向提交與逆序補償皆維持跨服務一致性！");
        }
        else
        {
            Console.WriteLine("⚠️ Saga 示範實驗結果異常，請檢查 SagaLab.cs！");
        }
        Console.WriteLine("=========================================================================");
    }

    private static bool RunSagaExperiment1_HappyPath()
    {
        Console.WriteLine("--- Saga 實驗 1：正向全通（建單 ➔ 扣庫存 ➔ 扣款 ➔ 確認訂單） ---");
        var orderService = new SagaOrderService();
        var inventoryService = new SagaInventoryService();
        inventoryService.SeedStock("機械鍵盤", 10);
        var paymentService = new SagaPaymentService { ShouldFail = false };

        var orchestrator = new OrderCheckoutSagaOrchestrator(orderService, inventoryService, paymentService);
        var result = orchestrator.ExecuteCheckout("Alice", "機械鍵盤", 1, 3500m);

        var order = result.OrderId.HasValue ? orderService.GetOrder(result.OrderId.Value) : null;
        int remainingStock = inventoryService.GetStock("機械鍵盤");
        bool passed = result.IsSuccess
            && order is { Status: SagaOrderStatus.Confirmed }
            && remainingStock == 9
            && paymentService.SuccessfulChargeCount == 1;

        Console.WriteLine($"📊 [Saga 實驗 1 稽核] 成功 = {result.IsSuccess} | 訂單狀態 = {order?.Status} | 剩餘庫存 = {remainingStock} | 扣款次數 = {paymentService.SuccessfulChargeCount}");
        Console.WriteLine(passed ? "✅ 通過：跨服務四步驟全部完成並確認成立！\n" : "❌ 失敗：正向 Saga 狀態不符合預期！\n");
        return passed;
    }

    private static bool RunSagaExperiment2_PaymentFailureCompensation()
    {
        Console.WriteLine("--- Saga 實驗 2：中途扣款失敗 ➔ LIFO 逆序觸發補償（釋放庫存 ➔ 取消訂單） ---");
        var orderService = new SagaOrderService();
        var inventoryService = new SagaInventoryService();
        inventoryService.SeedStock("限量顯卡", 10);
        var paymentService = new SagaPaymentService { ShouldFail = true };

        var orchestrator = new OrderCheckoutSagaOrchestrator(orderService, inventoryService, paymentService);
        var result = orchestrator.ExecuteCheckout("Bob", "限量顯卡", 1, 35000m);

        var order = result.OrderId.HasValue ? orderService.GetOrder(result.OrderId.Value) : null;
        int remainingStock = inventoryService.GetStock("限量顯卡");
        bool passed = !result.IsSuccess
            && order is { Status: SagaOrderStatus.Cancelled }
            && remainingStock == 10
            && paymentService.SuccessfulChargeCount == 0;

        Console.WriteLine($"📊 [Saga 實驗 2 稽核] 成功 = {result.IsSuccess} | 訂單狀態 = {order?.Status} | 取消原因 = {order?.CancelReason} | 剩餘庫存 = {remainingStock} | 扣款次數 = {paymentService.SuccessfulChargeCount}");
        Console.WriteLine(passed ? "✅ 通過：扣款失敗後依 LIFO 逆序補回庫存並語意取消訂單，無殘留髒資料！\n" : "❌ 失敗：Saga 補償結果不符合預期！\n");
        return passed;
    }
}
