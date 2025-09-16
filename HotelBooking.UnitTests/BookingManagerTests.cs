using System;
using System.Collections.Generic;
using HotelBooking.Core;
using Xunit;
using System.Threading.Tasks;
using HotelBooking.Core.Services;
using HotelBooking.UnitTests.Data;
using Moq;


namespace HotelBooking.UnitTests;

public class BookingManagerTests
{
    private readonly IBookingManager bookingManager;
    private readonly Mock<IRepository<Booking>> bookingRepository;

    public BookingManagerTests()
    {
        var fullyOccupiedStartDate = DateTime.Today.AddDays(10);
        var fullyOccupiedEndDate = DateTime.Today.AddDays(20);

        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2), IsActive = true,
                CustomerId = 1, RoomId = 1
            },
            new()
            {
                Id = 1, StartDate = fullyOccupiedStartDate, EndDate = fullyOccupiedEndDate, IsActive = true,
                CustomerId = 1, RoomId = 1
            },
            new()
            {
                Id = 2, StartDate = fullyOccupiedStartDate, EndDate = fullyOccupiedEndDate, IsActive = true,
                CustomerId = 2, RoomId = 2
            },
        };

        bookingRepository = new Mock<IRepository<Booking>>();
        bookingRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(bookings);
        bookingRepository.Setup(x => x.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

        var roomRepository = new Mock<IRepository<Room>>();

        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "A" },
            new() { Id = 2, Description = "B" },
        };
        roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object);
    }

    [Theory]
    [InlineData(1, 2, true)]
    [InlineData(25, 26, true)]
    [InlineData(21, 22, true)]
    [InlineData(9, 10, false)]
    public async Task CreateBooking_VariousDateRanges_ReturnsExpectedAvailability(int startOffset, int endOffset,
        bool expectedResult)
    {
        // Arrange
        var booking = new Booking
        {
            StartDate = DateTime.Today.AddDays(startOffset),
            EndDate = DateTime.Today.AddDays(endOffset),
            CustomerId = 1
        };

        // Act
        var result = await bookingManager.CreateBooking(booking);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [ClassData(typeof(BookingTestData))]
    public async Task CreateBooking_WithComplexScenarios_ReturnsExpectedResult(Booking booking, bool expectedResult)
    {
        // Act
        var result = await bookingManager.CreateBooking(booking);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
    {
        // Arrange
        var date = DateTime.Today.AddDays(1);
        // Act
        var roomId = await bookingManager.FindAvailableRoom(date, date);
        // Assert
        Assert.NotEqual(-1, roomId);
    }

    [Fact]
    public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
    {
        // This test was added to satisfy the following test design
        // principle: "Tests should have strong assertions".

        // Arrange
        var date = DateTime.Today.AddDays(1);

        // Act
        var roomId = await bookingManager.FindAvailableRoom(date, date);

        // Assert
        Assert.Equal(2, roomId);
    }

    [Theory]
    [InlineData(5, 4)] // end date before start date
    [InlineData(-1, 1)] // start date in the past
    public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException(int startOffset,
        int endOffset)
    {
        // Arrange
        DateTime start = DateTime.Today.AddDays(startOffset);
        DateTime end = DateTime.Today.AddDays(endOffset);

        // Act
        Task result() => bookingManager.FindAvailableRoom(start, end);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
    
    [Theory]
    [ClassData(typeof(DateRangeTestData))]
    public async Task GetFullyOccupiedDates_WithVariousDateRanges_ReturnsCorrectCount(DateTime startDate,
        DateTime endDate, int expectedCount)
    {
        // Act
        var result = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
    {
        // Arrange
        var emptyBookingRepository = new Mock<IRepository<Booking>>();
        emptyBookingRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Booking>());

        var roomRepository = new Mock<IRepository<Room>>();
        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "A" },
            new() { Id = 2, Description = "B" },
        };
        roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);

        var bookingManagerWithNoBookings = new BookingManager(emptyBookingRepository.Object,
            roomRepository.Object);

        var start = DateTime.Today.AddDays(1);
        var end = DateTime.Today.AddDays(30);

        // Act
        var result = await bookingManagerWithNoBookings.GetFullyOccupiedDates(start, end);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_BookingsExist_ReturnsCorrectFullyOccupiedDates()
    {
        // Arrange
        var start = DateTime.Today;
        var end = DateTime.Today.AddDays(30);
        var expectedFullyOccupiedDates = new List<DateTime>();
        for (var d = DateTime.Today.AddDays(10); d <= DateTime.Today.AddDays(20); d = d.AddDays(1))
        {
            expectedFullyOccupiedDates.Add(d);
        }

        // Act
        var result = await bookingManager.GetFullyOccupiedDates(start, end);

        // Assert
        Assert.Equal(expectedFullyOccupiedDates, result);
    }

    [Fact]
    public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException()
    {
        // Arrange
        var start = DateTime.Today.AddDays(10);
        var end = DateTime.Today;

        // Act
        Task result() => bookingManager.GetFullyOccupiedDates(start, end);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
}