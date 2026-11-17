using System;
using Xunit;
using Stage4_SystemDesign;

namespace Stage4_SystemDesign.Tests
{
    public class ReservationTests
    {
        [Fact]
        public void CalculateTotalAmount_ThreeNights_ReturnsCorrectAmount()
        {
            // Arrange (準備測試資料：每晚 2000 元，住 3 晚)
            var roomType = new RoomType("標準雙人房", 2000m);
            var room = new Room("201", roomType);
            var guest = new Guest("王小明", "ming@test.com");
            var checkIn = new DateTime(2026, 12, 1);
            var checkOut = new DateTime(2026, 12, 4);
            var reservation = new Reservation("RES-001", guest, room, checkIn, checkOut, new FlexibleRefundPolicy());

            // Act (執行業務邏輯 - Tell, Don't Ask)
            int nights = reservation.CalculateNights();
            decimal total = reservation.CalculateTotalAmount();

            // Assert (驗證結果)
            Assert.Equal(3, nights);
            Assert.Equal(6000m, total);
        }

        [Fact]
        public void Cancel_FiveDaysBeforeCheckIn_ReturnsFullRefund()
        {
            // Arrange (準備測試資料：總額 6000 元，2026-12-01 入住)
            var roomType = new RoomType("標準雙人房", 2000m);
            var room = new Room("201", roomType);
            var guest = new Guest("王小明", "ming@test.com");
            var checkIn = new DateTime(2026, 12, 1);
            var checkOut = new DateTime(2026, 12, 4);
            var reservation = new Reservation("RES-001", guest, room, checkIn, checkOut, new FlexibleRefundPolicy());

            // Act (在入住前 5 天：2026-11-26 取消)
            var cancelDate = new DateTime(2026, 11, 26);
            decimal refund = reservation.Cancel(cancelDate);

            // Assert (寬鬆政策 3 天前取消全退：應退 6000 元，且狀態變更為 Cancelled)
            Assert.Equal(6000m, refund);
            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
            Assert.Equal(RoomStatus.Available, room.Status);
        }

        [Fact]
        public void Cancel_OneDayBeforeCheckIn_ReturnsFiftyPercentRefund()
        {
            // Arrange (總額 6000 元，2026-12-01 入住)
            var roomType = new RoomType("標準雙人房", 2000m);
            var room = new Room("201", roomType);
            var guest = new Guest("王小明", "ming@test.com");
            var checkIn = new DateTime(2026, 12, 1);
            var checkOut = new DateTime(2026, 12, 4);
            var reservation = new Reservation("RES-001", guest, room, checkIn, checkOut, new FlexibleRefundPolicy());

            // Act (在入住前 1 天：2026-11-30 取消)
            var cancelDate = new DateTime(2026, 11, 30);
            decimal refund = reservation.Cancel(cancelDate);

            // Assert (3 天內取消退 50%：應退 3000 元)
            Assert.Equal(3000m, refund);
            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        }

        [Fact]
        public void Cancel_NonRefundablePolicy_ReturnsZeroRefund()
        {
            // Arrange (使用「不可退款特惠政策」)
            var roomType = new RoomType("早鳥特惠房", 1500m);
            var room = new Room("101", roomType);
            var guest = new Guest("李小華", "hua@test.com");
            var checkIn = new DateTime(2026, 12, 1);
            var checkOut = new DateTime(2026, 12, 4);
            var reservation = new Reservation("RES-002", guest, room, checkIn, checkOut, new NonRefundablePolicy());

            // Act (即便提早 10 天取消)
            var cancelDate = new DateTime(2026, 11, 20);
            decimal refund = reservation.Cancel(cancelDate);

            // Assert (特惠政策一律不退款：0 元)
            Assert.Equal(0m, refund);
        }

        [Fact]
        public void Constructor_CheckOutBeforeCheckIn_ThrowsArgumentException()
        {
            // Arrange
            var roomType = new RoomType("標準雙人房", 2000m);
            var room = new Room("201", roomType);
            var guest = new Guest("王小明", "ming@test.com");
            var checkIn = new DateTime(2026, 12, 4);
            var invalidCheckOut = new DateTime(2026, 12, 1); // ❌ 惡意或錯誤資料：退房在入住前

            // Act & Assert (守門員防線：驗證是否正確拋出例外)
            Assert.Throws<ArgumentException>(() =>
            {
                new Reservation("RES-ERR", guest, room, checkIn, invalidCheckOut, new FlexibleRefundPolicy());
            });
        }
    }
}
