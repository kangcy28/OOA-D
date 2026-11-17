using System;

namespace Stage4_SystemDesign
{
    // =========================================================================
    // 列舉型態 (Enums)
    // =========================================================================

    public enum RoomStatus
    {
        Available,
        Occupied,
        Maintenance
    }

    public enum ReservationStatus
    {
        PendingPayment,
        Confirmed,
        Cancelled
    }

    // =========================================================================
    // 實體 1：房型 (RoomType) —— 資訊專家：擁有每晚單價
    // =========================================================================

    public class RoomType
    {
        public string Name { get; }
        public decimal PricePerNight { get; }

        public RoomType(string name, decimal pricePerNight)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("房型名稱不能為空");
            if (pricePerNight <= 0) throw new ArgumentException("每晚房價必須大於 0");

            Name = name;
            PricePerNight = pricePerNight;
        }
    }

    // =========================================================================
    // 實體 2：客房 (Room) —— 聚合 RoomType
    // =========================================================================

    public class Room
    {
        public string RoomNumber { get; }
        public RoomType Type { get; }
        public RoomStatus Status { get; private set; }

        public Room(string roomNumber, RoomType type)
        {
            RoomNumber = roomNumber ?? throw new ArgumentNullException(nameof(roomNumber));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Status = RoomStatus.Available;
        }

        public void MarkAsOccupied() => Status = RoomStatus.Occupied;
        public void MarkAsAvailable() => Status = RoomStatus.Available;
    }

    // =========================================================================
    // 實體 3：客戶 (Guest)
    // =========================================================================

    public class Guest
    {
        public string Name { get; }
        public string Email { get; }

        public Guest(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("姓名不能為空");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email 不能為空");

            Name = name;
            Email = email;
        }
    }

    // =========================================================================
    // 策略模式：退款政策抽象與具體策略 (Strategy Pattern)
    // =========================================================================

    public interface IRefundPolicy
    {
        decimal CalculateRefund(decimal totalPaid, DateTime checkInDate, DateTime cancellationDate);
    }

    /// <summary>
    /// 寬鬆退款政策：入住 3 天前取消全額退款，3 天內退 50%
    /// </summary>
    public class FlexibleRefundPolicy : IRefundPolicy
    {
        public decimal CalculateRefund(decimal totalPaid, DateTime checkInDate, DateTime cancellationDate)
        {
            int daysBeforeCheckIn = (checkInDate.Date - cancellationDate.Date).Days;

            if (daysBeforeCheckIn >= 3)
            {
                return totalPaid; // 100% 全退
            }
            else if (daysBeforeCheckIn >= 0)
            {
                return totalPaid * 0.5m; // 50% 半退
            }
            else
            {
                return 0m; // 已過入住日，不退款
            }
        }
    }

    /// <summary>
    /// 不可退款特惠政策：取消一律 0 元退款
    /// </summary>
    public class NonRefundablePolicy : IRefundPolicy
    {
        public decimal CalculateRefund(decimal totalPaid, DateTime checkInDate, DateTime cancellationDate)
        {
            return 0m;
        }
    }

    // =========================================================================
    // 聚合根實體：預訂單 (Reservation) —— 核心業務邏輯與狀態守門員
    // =========================================================================

    public class Reservation
    {
        public string Id { get; }
        public Guest Guest { get; }
        public Room Room { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public ReservationStatus Status { get; private set; }
        public IRefundPolicy RefundPolicy { get; private set; }

        public Reservation(string id, Guest guest, Room room, DateTime checkInDate, DateTime checkOutDate, IRefundPolicy refundPolicy)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("預訂單號不能為空");
            if (checkOutDate <= checkInDate) throw new ArgumentException("退房日期必須在入住日期之後！");

            Id = id;
            Guest = guest ?? throw new ArgumentNullException(nameof(guest));
            Room = room ?? throw new ArgumentNullException(nameof(room));
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            RefundPolicy = refundPolicy ?? throw new ArgumentNullException(nameof(refundPolicy));
            Status = ReservationStatus.PendingPayment;
        }

        // ✅ Tell, Don't Ask：計算住宿晚數
        public int CalculateNights()
        {
            return (CheckOutDate.Date - CheckInDate.Date).Days;
        }

        // ✅ 資訊專家：晚數 * 房型每晚定價
        public decimal CalculateTotalAmount()
        {
            return CalculateNights() * Room.Type.PricePerNight;
        }

        // 確認付款
        public void ConfirmPayment()
        {
            if (Status != ReservationStatus.PendingPayment)
                throw new InvalidOperationException($"目前狀態為 {Status}，無法執行確認付款");

            Status = ReservationStatus.Confirmed;
            Room.MarkAsOccupied();
        }

        // ✅ 取消預訂並回傳應退款金額
        public decimal Cancel(DateTime cancellationDate)
        {
            if (Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("此預訂單已經取消過，不可重複取消");

            Status = ReservationStatus.Cancelled;
            Room.MarkAsAvailable();

            // 吩咐退款策略計算退款金額 (Tell, Don't Ask)
            decimal totalPaid = CalculateTotalAmount();
            return RefundPolicy.CalculateRefund(totalPaid, CheckInDate, cancellationDate);
        }
    }
}
