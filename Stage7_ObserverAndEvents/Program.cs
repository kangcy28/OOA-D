using System;
using System.Collections.Generic;

namespace Stage7_ObserverAndEvents
{
    // =========================================================================
    // 【任務 7.1 反面教材】：高耦合上帝 Service
    // 痛點：當「訂單成立」伴隨越來越多的跨領域副作用時，BadOrderService 的建構子會不斷膨脹，
    // 違反單一職責 (SRP) 與開放封閉 (OCP)，且單元測試必須 Mock 一整排外部依賴！
    // =========================================================================

    public class LegacyInventoryService
    {
        public void DeductStock(string orderId) => Console.WriteLine($"  [舊版庫存] 訂單 {orderId} 扣除商品庫存。");
    }

    public class LegacyLoyaltyPointsService
    {
        public void AddPoints(string customerEmail, decimal amount) =>
            Console.WriteLine($"  [舊版點數] 為 {customerEmail} 累計消費點數 {(int)(amount / 10)} 點。");
    }

    public class LegacyInvoiceService
    {
        public void IssueInvoice(string orderId, decimal amount) =>
            Console.WriteLine($"  [舊版發票] 開立訂單 {orderId} 電子發票，金額 ${amount}。");
    }

    public class LegacyEmailService
    {
        public void SendConfirmation(string customerEmail, string orderId) =>
            Console.WriteLine($"  [舊版郵件] 寄送訂單 {orderId} 確認信至 {customerEmail}。");
    }

    public class BadOrderService
    {
        private readonly LegacyInventoryService _inventory;
        private readonly LegacyLoyaltyPointsService _points;
        private readonly LegacyInvoiceService _invoice;
        private readonly LegacyEmailService _email;

        public BadOrderService(
            LegacyInventoryService inventory,
            LegacyLoyaltyPointsService points,
            LegacyInvoiceService invoice,
            LegacyEmailService email)
        {
            _inventory = inventory;
            _points = points;
            _invoice = invoice;
            _email = email;
        }

        public void PlaceOrder(string orderId, string customerEmail, decimal totalAmount)
        {
            Console.WriteLine($"[舊版核心] 建立訂單 {orderId}，金額 ${totalAmount}...");

            // 核心流程直接硬編碼依賴了 4 個跨領域服務！
            // 每新增一個「風控檢查」或「數據埋點」，這裡與建構子就必須被迫修改。
            _inventory.DeductStock(orderId);
            _points.AddPoints(customerEmail, totalAmount);
            _invoice.IssueInvoice(orderId, totalAmount);
            _email.SendConfirmation(customerEmail, orderId);
        }
    }

    // =========================================================================
    // 【任務 7.2 正確架構】：觀察者模式 & 領域事件 (Observer / Domain Events)
    // 核心精神：將「已經發生的事實」封裝成不可變事件 (OrderPlacedEvent)，
    // OrderService 只負責發布事件；所有跨領域副作用由獨立訂閱者自行訂閱響應。
    // =========================================================================

    /// <summary>
    /// 領域事件（不可變值物件）：紀錄「訂單已成立」的事實快照
    /// </summary>
    public sealed record OrderPlacedEvent(
        string OrderId,
        string CustomerEmail,
        decimal TotalAmount,
        DateTime OccurredAt);

    /// <summary>
    /// 訂閱者抽象契約 (Observer Interface)
    /// </summary>
    public interface IOrderPlacedHandler
    {
        string HandlerName { get; }
        void Handle(OrderPlacedEvent domainEvent);
    }

    /// <summary>
    /// 訂閱者 1：庫存扣減處理器
    /// </summary>
    public class InventoryHandler : IOrderPlacedHandler
    {
        public string HandlerName => "庫存服務 (InventoryHandler)";

        public void Handle(OrderPlacedEvent domainEvent)
        {
            Console.WriteLine($"  ➔ [{HandlerName}] 收到事件，扣除訂單 {domainEvent.OrderId} 庫存。");
        }
    }

    /// <summary>
    /// 訂閱者 2：會員點數發放處理器
    /// </summary>
    public class LoyaltyPointsHandler : IOrderPlacedHandler
    {
        public string HandlerName => "會員點數服務 (LoyaltyPointsHandler)";

        public void Handle(OrderPlacedEvent domainEvent)
        {
            int earnedPoints = (int)(domainEvent.TotalAmount / 10);
            Console.WriteLine($"  ➔ [{HandlerName}] 收到事件，為 {domainEvent.CustomerEmail} 發放 {earnedPoints} 點。");
        }
    }

    /// <summary>
    /// 訂閱者 3：電子發票開立處理器
    /// </summary>
    public class InvoiceHandler : IOrderPlacedHandler
    {
        public string HandlerName => "電子發票服務 (InvoiceHandler)";

        public void Handle(OrderPlacedEvent domainEvent)
        {
            Console.WriteLine($"  ➔ [{HandlerName}] 收到事件，開立訂單 {domainEvent.OrderId} 發票 (${domainEvent.TotalAmount})。");
        }
    }
    /// <summary>
    /// 訂閱者 4：郵件寄送處理器
    /// </summary>
    public class EmailHandler : IOrderPlacedHandler
    {
        public string HandlerName => "郵件服務 (EmailHandler)";

        public void Handle(OrderPlacedEvent domainEvent)
        {
            Console.WriteLine($"  ➔ [{HandlerName}] 收到事件，寄送訂單 {domainEvent.OrderId} 的確認郵件 (${domainEvent.TotalAmount})。");
        }
    }

    /// <summary>
    /// 發布者 (Publisher)：徹底解耦的 OrderService，只專注訂單建立與事件廣播，
    /// 完全不認識任何具體的下游服務！
    /// </summary>
    public class OrderService
    {
        private readonly List<IOrderPlacedHandler> _handlers = new();

        public OrderService(IEnumerable<IOrderPlacedHandler>? initialHandlers = null)
        {
            if (initialHandlers != null)
            {
                _handlers.AddRange(initialHandlers);
            }
        }

        /// <summary>
        /// 支援執行期動態掛載新的訂閱者（Observer Pattern 的經典註冊機制）
        /// </summary>
        public void Subscribe(IOrderPlacedHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
        }

        public void Unsubscribe(IOrderPlacedHandler handler)
        {
            _handlers.Remove(handler);
        }

        public void PlaceOrder(string orderId, string customerEmail, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("訂單編號不可為空", nameof(orderId));
            if (totalAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalAmount), "訂單金額必須大於零");

            Console.WriteLine($"[核心領域] 訂單 {orderId} 建立並儲存成功！（金額：${totalAmount}）");

            // 發布領域事件（通知所有訂閱者）
            var domainEvent = new OrderPlacedEvent(orderId, customerEmail, totalAmount, DateTime.UtcNow);
            Publish(domainEvent);
        }

        private void Publish(OrderPlacedEvent domainEvent)
        {
            Console.WriteLine($"[事件廣播] 發布 OrderPlacedEvent (OrderId: {domainEvent.OrderId}) 給 {_handlers.Count} 位訂閱者：");
            foreach (var handler in _handlers)
            {
                handler.Handle(domainEvent);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 階段七：事件驅動與解耦實戰 (Observer / Domain Events) ===\n");

            // 1. 建置純淨的 OrderService（初始訂閱：庫存、點數、發票）
            var orderService = new OrderService(new IOrderPlacedHandler[]
            {
                new InventoryHandler(),
                new LoyaltyPointsHandler(),
                new InvoiceHandler(),                
                new EmailHandler()
            });

            Console.WriteLine("1. 執行第一筆下單（4 位初始訂閱者響應）：");
            orderService.PlaceOrder("ORD-2026-001", "vip@example.com", 1500m);

            // 2. 動態掛載新訂閱者（展示執行期擴充，OrderService 本體零修改）
            Console.WriteLine("\n2. 動態註冊新訂閱者（例如：臨時活動通知）後執行第二筆下單：");
            var campaignNotifier = new InlineNotificationHandler("雙十一限時回饋通知");
            orderService.Subscribe(campaignNotifier);
            orderService.PlaceOrder("ORD-2026-002", "alex@example.com", 2800m);
        }
    }

    /// <summary>
    /// 示範用簡易訂閱者：展示執行期可自由 Subscribe / Unsubscribe
    /// </summary>
    public class InlineNotificationHandler : IOrderPlacedHandler
    {
        private readonly string _campaignTitle;

        public InlineNotificationHandler(string campaignTitle)
        {
            _campaignTitle = campaignTitle;
        }

        public string HandlerName => $"行銷推播 ({_campaignTitle})";

        public void Handle(OrderPlacedEvent domainEvent)
        {
            Console.WriteLine($"  ➔ [{HandlerName}] 收到事件，發送「{_campaignTitle}」優惠券給 {domainEvent.CustomerEmail}！");
        }
    }
}
