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

    // 思考題：如果 VIP 打 9 折、滿千折 100、生日再折 50 要能「任意順序、自由疊加」：
    // 我們在任務 5.3 將用裝飾者模式 (Decorator Pattern) 讓每個優惠包著上一個優惠！
    // 例如：new BirthdayDiscount(new OverThousandDiscount(new VipDiscount(new BasePrice())))

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== 階段五：進階 OOA/D 面試實戰 (Value Object & Decorator) ===\n");

            var stayA = new DateRange(new DateTime(2026, 12, 1), new DateTime(2026, 12, 4)); // 12/1 ~ 12/4
            var stayB = new DateRange(new DateTime(2026, 12, 3), new DateTime(2026, 12, 6)); // 12/3 ~ 12/6 (重疊)
            var stayC = new DateRange(new DateTime(2026, 12, 4), new DateTime(2026, 12, 6)); // 12/4 ~ 12/6 (無縫接軌)

            Console.WriteLine($"住宿 A 天數：{stayA.Days} 晚");
            Console.WriteLine($"A (12/1~12/4) 與 B (12/3~12/6) 是否衝突？ {stayA.OverlapsWith(stayB)}");
            Console.WriteLine($"A (12/1~12/4) 與 C (12/4~12/6) 是否衝突？ {stayA.OverlapsWith(stayC)}");

            var copyOfA = new DateRange(new DateTime(2026, 12, 1), new DateTime(2026, 12, 4));
            Console.WriteLine($"值物件相等性檢查 (stayA.Equals(copyOfA))： {stayA.Equals(copyOfA)}");
        }
    }
}
