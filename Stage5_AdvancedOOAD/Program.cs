using System;

namespace Stage5_AdvancedOOAD
{
    // =========================================================================
    // 【任務 5.1 & 5.2】：消滅「基本型別偏執 (Primitive Obsession)」—— Value Object
    // =========================================================================

    /// <summary>
    /// ❌ 反面教材：到處散落的日期驗證與衝突檢查（Primitive Obsession）
    /// 每次有「訂房」、「預約會議室」、「請假系統」，工程師都在重複寫 Start < End 與重疊公式。
    /// </summary>
    public class BadReservationConflictChecker
    {
        public static bool IsOverlapping(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            if (endA <= startA || endB <= startB)
                throw new ArgumentException("結束日期必須大於開始日期");

            // 如果每個 Service 都自己寫一遍，極易寫錯邊界（例如 <= vs <）
            return startA < endB && startB < endA;
        }
    }

    /// <summary>
    /// ✅ 【任務 5.2 練習骨架】：不可變的值物件 (Value Object) — DateRange
    /// 特徵：
    /// 1. 沒有唯一 ID（只要 Start 與 End 相同，兩個 DateRange 就相等）。
    /// 2. 不可變 (Immutable)：屬性唯讀，建構子一次驗證完成。
    /// 3. 資訊專家：天數計算與「區間重疊判定 (OverlapsWith)」都封裝在自己身上！
    /// </summary>
    public class DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("結束日期必須大於開始日期！");

            Start = start.Date;
            End = end.Date;
        }

        public int Days => (End - Start).Days;

        // 🎯 【任務 5.2 挑戰】：請思考並驗證這個核心重疊判斷條件！
        // 提示：入住 12/01 ~ 退房 12/04 與 入住 12/04 ~ 退房 12/06，算不算衝突？（飯店退房當天可讓下組客人入住）
        public bool OverlapsWith(DateRange other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            // TODO: 確認並理解為何半開區間 [Start, End) 的重疊條件是 Start < other.End && other.Start < End
            return Start < other.End && other.Start < End;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not DateRange other) return false;
            return Start == other.Start && End == other.End;
        }

        public override int GetHashCode() => HashCode.Combine(Start, End);
    }

    // =========================================================================
    // 【任務 5.3 預告】：面試官追問 —— 多重優惠如何動態疊加？ (Decorator Pattern)
    // =========================================================================

    public interface IDiscount
    {
        decimal Apply(decimal amount);
    }

    public class BasePrice : IDiscount
    {
        public decimal Apply(decimal amount) => amount;
    }

    /// <summary>
    /// 裝飾者基底抽象類別：實作 IDiscount，並在內部聚合另一個 IDiscount
    /// </summary>
    public abstract class DiscountDecorator : IDiscount
    {
        protected readonly IDiscount _inner;

        public DiscountDecorator(IDiscount inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public virtual decimal Apply(decimal amount)
        {
            var currentAmount = _inner.Apply(amount);
            return ApplyDiscount(currentAmount);
        }

        protected abstract decimal ApplyDiscount(decimal currentAmount);
    }

    /// <summary>
    /// VIP 會員：打 9 折
    /// </summary>
    public class VipDiscount : DiscountDecorator
    {
        public VipDiscount(IDiscount inner) : base(inner) { }

        protected override decimal ApplyDiscount(decimal currentAmount)
            => currentAmount * 0.9m;
    }

    /// <summary>
    /// 滿千折百：若當前金額滿 1000 元，折抵 100 元
    /// </summary>
    public class OverThousandDiscount : DiscountDecorator
    {
        public OverThousandDiscount(IDiscount inner) : base(inner) { }

        protected override decimal ApplyDiscount(decimal currentAmount)
            => currentAmount >= 1000m ? currentAmount - 100m : currentAmount;
    }

    /// <summary>
    /// 壽星優惠：現折 50 元（最低不小於 0）
    /// </summary>
    public class BirthdayDiscount : DiscountDecorator
    {
        public BirthdayDiscount(IDiscount inner) : base(inner) { }

        protected override decimal ApplyDiscount(decimal currentAmount)
            => Math.Max(0m, currentAmount - 50m);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 階段五：進階 OOA/D 面試實戰 (Value Object & Decorator) ===\n");

            // 1. DateRange 測試
            var stayA = new DateRange(new DateTime(2026, 12, 1), new DateTime(2026, 12, 4)); // 12/1 ~ 12/4
            var stayB = new DateRange(new DateTime(2026, 12, 3), new DateTime(2026, 12, 6)); // 12/3 ~ 12/6 (重疊)
            var stayC = new DateRange(new DateTime(2026, 12, 4), new DateTime(2026, 12, 6)); // 12/4 ~ 12/6 (無縫接軌)

            Console.WriteLine($"住宿 A 天數：{stayA.Days} 晚");
            Console.WriteLine($"A (12/1~12/4) 與 B (12/3~12/6) 是否衝突？ {stayA.OverlapsWith(stayB)}");
            Console.WriteLine($"A (12/1~12/4) 與 C (12/4~12/6) 是否衝突？ {stayA.OverlapsWith(stayC)}");

            var copyOfA = new DateRange(new DateTime(2026, 12, 1), new DateTime(2026, 12, 4));
            Console.WriteLine($"值物件相等性檢查 (stayA.Equals(copyOfA))： {stayA.Equals(copyOfA)}\n");

            // 2. 裝飾者模式 (多重優惠動態疊加) 驗證
            decimal originalPrice = 1000m;
            Console.WriteLine($"--- 原價：${originalPrice} ---");

            // 方案 A：先算滿千折 100 ➔ 再打 VIP 9 折
            IDiscount discountA = new VipDiscount(new OverThousandDiscount(new BasePrice()));
            Console.WriteLine($"方案 A (先滿千折百 ➔ 再打 VIP 9折)：${discountA.Apply(originalPrice)}");

            // 方案 B：先打 VIP 9 折 ➔ 再算滿千折 100
            IDiscount discountB = new OverThousandDiscount(new VipDiscount(new BasePrice()));
            Console.WriteLine($"方案 B (先打 VIP 9折 ➔ 再滿千折百)：${discountB.Apply(originalPrice)}");

            // 方案 C：全部疊加（VIP 9折 ➔ 滿千折百 ➔ 壽星折 50）原價 1500
            decimal higherPrice = 1500m;
            IDiscount allInOne = new BirthdayDiscount(new OverThousandDiscount(new VipDiscount(new BasePrice())));
            Console.WriteLine($"原價 ${higherPrice} 全疊加 (VIP➔滿千折百➔壽星)：${allInOne.Apply(higherPrice)}");
        }
    }
}
