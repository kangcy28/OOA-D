using System;
using System.Collections.Generic;

namespace Stage3_Playground
{
    // =========================================================================
    // 【任務 3.1 成果】：遵循 GRASP 資訊專家模式 (Information Expert)
    // =========================================================================
    
    /// <summary>
    /// 訂單明細：自己擁有數量與單價，因此是計算小計的「資訊專家」
    /// </summary>
    public class OrderItem
    {
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal PurchasePrice { get; }

        public OrderItem(string productName, int quantity, decimal purchasePrice)
        {
            if (quantity <= 0) throw new ArgumentException("數量必須大於 0");
            if (purchasePrice < 0) throw new ArgumentException("單價不能為負數");

            ProductName = productName;
            Quantity = quantity;
            PurchasePrice = purchasePrice;
        }

        // ✅ 資訊專家：OrderItem 擁有所有計算小計所需的資訊
        public decimal CalculateSubtotal()
        {
            return Quantity * PurchasePrice;
        }
    }

    // =========================================================================
    // 【任務 3.2 成果】：OCP (開放封閉原則) 與 Strategy Pattern (策略模式)
    // =========================================================================

    /// <summary>
    /// 折扣策略通用抽象契約：只要是折扣策略，都必須遵守此行為
    /// </summary>
    public interface IDiscountStrategy
    {
        // 不管你是哪種活動，傳入「原價」，算完後回傳「折扣後的實付金額」
        decimal ApplyDiscount(decimal originalAmount);
    }

    /// <summary>
    /// 不打折策略（空物件模式 Null Object Pattern）
    /// </summary>
    public class NoneDiscount : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal originalAmount)
        {
            return originalAmount;
        }
    }

    /// <summary>
    /// VIP 專屬策略（打 9 折）
    /// </summary>
    public class VipDiscount : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal originalAmount)
        {
            return originalAmount * 0.9m;
        }
    }

    /// <summary>
    /// 滿千折百策略
    /// </summary>
    public class OverThousandDiscount : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal originalAmount)
        {
            return originalAmount >= 1000 ? originalAmount - 100m : originalAmount;
        }
    }

    /// <summary>
    /// 黑五狂歡策略（現折 500 元）—— 完全不改動 Order 即可擴充！
    /// </summary>
    public class BlackFridayDiscount : IDiscountStrategy
    {
        public decimal ApplyDiscount(decimal originalAmount)
        {
            decimal total = originalAmount - 500m;
            return total < 0 ? 0 : total;
        }
    }

    public class Order
    {
        private readonly List<OrderItem> _items = new List<OrderItem>();

        public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

        public void AddItem(OrderItem item)
        {
            _items.Add(item);
        }

        // ✅ 資訊專家：Order 擁有所有的 OrderItem，自己算未折扣前的基本總額
        public decimal CalculateBaseTotal()
        {
            decimal sum = 0;
            foreach (var item in _items)
            {
                sum += item.CalculateSubtotal(); // Tell, Don't Ask
            }
            return sum;
        }

        // ✅ 【好設計】：完全符合 OCP 與 SRP！
        // Order 完全不需要管世上有哪些活動、更不需要寫任何 if-else。
        // 它只依賴抽象介面 IDiscountStrategy，新活動來了完全不用動這裡！
        public decimal CalculateFinalTotal(IDiscountStrategy discount)
        {
            if (discount == null) throw new ArgumentNullException(nameof(discount));

            decimal baseTotal = CalculateBaseTotal();
            decimal total = discount.ApplyDiscount(baseTotal);
            return total < 0 ? 0 : total;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 階段三：OOD 職責分配與彈性架構實戰 (策略模式) ===\n");

            var order = new Order();
            order.AddItem(new OrderItem("機械式鍵盤", 1, 2500m));
            order.AddItem(new OrderItem("人體工學滑鼠", 2, 800m));

            Console.WriteLine($"訂單商品原始金額：{order.CalculateBaseTotal()} 元\n");

            // ✅ 使用多型傳入各個獨立的策略物件
            Console.WriteLine($"[無折扣] 實付金額：{order.CalculateFinalTotal(new NoneDiscount())} 元");
            Console.WriteLine($"[VIP 9折] 實付金額：{order.CalculateFinalTotal(new VipDiscount())} 元");
            Console.WriteLine($"[滿千折百] 實付金額：{order.CalculateFinalTotal(new OverThousandDiscount())} 元");
            Console.WriteLine($"[黑色星期五] 實付金額：{order.CalculateFinalTotal(new BlackFridayDiscount())} 元");

            Console.WriteLine("\n✨ 成功！不管未來行銷部再加幾百種促銷，Order 類別永遠不需要修改！");
        }
    }
}
