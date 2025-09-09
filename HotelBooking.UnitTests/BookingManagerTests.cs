using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;

        public BookingManagerTests(){
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            bookingRepository = new FakeBookingRepository(start, end);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }
        
        [Theory]
        [InlineData(5, 4)]   // end date before start date
        [InlineData(-1, 1)]  // start date in the past
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException(int startOffset, int endOffset)
        {
            // Arrange
            DateTime start = DateTime.Today.AddDays(startOffset);
            DateTime end = DateTime.Today.AddDays(endOffset);

            // Act
            Task result() => bookingManager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }
        
        [Fact]
        public async Task CreateBooking_RoomNotAvailable_ReturnsFalse()
        {
            // Arrange
            var start = DateTime.Today.AddDays(15);
            var end = DateTime.Today.AddDays(16);
            var booking = new Booking
            {
                StartDate = start,
                EndDate = end,
                CustomerId = 1
            };

            // Act
            var result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task CreateBooking_RoomAvailable_ReturnsTrue()
        {
            // Arrange
            var start = DateTime.Today.AddDays(1);
            var end = DateTime.Today.AddDays(2);
            var booking = new Booking
            {
                StartDate = start,
                EndDate = end,
                CustomerId = 1
            };

            // Act
            var result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

    }
}
