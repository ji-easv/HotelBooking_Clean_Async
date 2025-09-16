using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class BookingsControllerTests
{
    private BookingsController controller;
    private Mock<IRepository<Booking>> mockBookingRepository;
    private Mock<IBookingManager> mockBookingManager;
    
    public BookingsControllerTests()
    {
        var bookings = new List<Booking>
        {
            new() { Id=1, RoomId=1, StartDate=DateTime.Now, EndDate=DateTime.Now.AddDays(2) },
            new() { Id=2, RoomId=2, StartDate=DateTime.Now, EndDate=DateTime.Now.AddDays(3) },
        };

        // Create mock BookingRepository. 
        mockBookingRepository = new Mock<IRepository<Booking>>();

        // Implement mock GetAll() method.
        mockBookingRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(bookings);

        // Implement mock Get() method.
        mockBookingRepository.Setup(x =>
        x.GetAsync(It.IsInRange<int>(1, 2, Moq.Range.Inclusive))).ReturnsAsync(bookings[1]);

        // Create a mock IBookingManager
        mockBookingManager = new Mock<IBookingManager>();
        mockBookingManager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(true);

        // Create BookingsController
        controller = new BookingsController(mockBookingRepository.Object, mockBookingManager.Object);
    }
    
    [Fact]
    public async Task GetAll_ReturnsListWithCorrectNumberOfBookings()
    {
        // Act
        var result = await controller.Get() as List<Booking>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Get_ExistingId_ReturnsCorrectBooking()
    {
        // Act
        var result = await controller.Get(2);
        
        // Assert
        Assert.IsType<ObjectResult>(result);
        Assert.NotNull(result);
        var booking = (result as ObjectResult)!.Value as Booking;
        Assert.NotNull(booking);
        Assert.Equal(2, booking.Id);
    }
    
    [Fact]
    public async Task Get_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await controller.Get(99);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_ValidBooking_ReturnsCreatedAtRoute()
    {
        var newBooking = new Booking
        {
            StartDate = DateTime.Now.AddDays(5),
            EndDate = DateTime.Now.AddDays(7)
        };
        
        // Act
        var result = await controller.Post(newBooking);
        
        // Assert
        Assert.IsType<CreatedAtRouteResult>(result);
        mockBookingManager.Verify(m => m.CreateBooking(It.IsAny<Booking>()), Times.Once);
    }
    
    [Fact]
    public async Task Post_NullBooking_ReturnsBadRequest()
    {
        // Act
        var result = await controller.Post(null);
        
        // Assert
        Assert.IsType<BadRequestResult>(result);
        mockBookingManager.Verify(m => m.CreateBooking(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Post_AllRoomsOccupied_ReturnsConflict()
    {
        // Arrange
        var newBooking = new Booking
        {
            StartDate = DateTime.Now.AddDays(5),
            EndDate = DateTime.Now.AddDays(7)
        };
        mockBookingManager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(false);
        
        // Act
        var result = await controller.Post(newBooking);
        
        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        mockBookingManager.Verify(m => m.CreateBooking(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task Put_ValidBooking_ReturnsNoContent()
    {
        // Arrange
        var updatedBooking = new Booking
        {
            Id = 2,
            StartDate = DateTime.Now.AddDays(1000),
            EndDate = DateTime.Now.AddDays(1001),
            CustomerId = 87,
            IsActive = true,
            RoomId = 1000
        };
        
        // Act
        var result = await controller.Put(2, updatedBooking);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
        mockBookingRepository.Verify(m => m.GetAsync(2), Times.Once);
        mockBookingRepository.Verify(m => m.EditAsync(It.IsAny<Booking>()), Times.Once);
        
        var booking = await mockBookingRepository.Object.GetAsync(2);
        Assert.Equal(87, booking.CustomerId);
        Assert.True(booking.IsActive);
        Assert.NotEqual(DateTime.Now.AddDays(1000), booking.StartDate);
        Assert.NotEqual(DateTime.Now.AddDays(1001), booking.EndDate);
        Assert.NotEqual(1000, booking.RoomId);
    }
    
    [Theory]
    [InlineData(2, null)] // Null booking
    [InlineData(2, 3)]    // Mismatched IDs
    public async Task Put_InvalidArguments_ReturnsBadRequest(int id, int? bookingId)
    {
        // Arrange
        Booking updatedBooking = null;
        if (bookingId.HasValue)
        {
            updatedBooking = new Booking { Id = bookingId.Value };
        }
        
        // Act
        var result = await controller.Put(id, updatedBooking);
        
        // Assert
        Assert.IsType<BadRequestResult>(result);
        mockBookingRepository.Verify(m => m.GetAsync(It.IsAny<int>()), Times.Never);
        mockBookingRepository.Verify(m => m.EditAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Put_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updatedBooking = new Booking { Id = 99 };
        mockBookingRepository.Setup(m => m.GetAsync(99)).ReturnsAsync((Booking)null);
        
        // Act
        var result = await controller.Put(99, updatedBooking);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockBookingRepository.Verify(m => m.GetAsync(99), Times.Once);
        mockBookingRepository.Verify(m => m.EditAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        // Act
        var result = await controller.Delete(2);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
        mockBookingRepository.Verify(m => m.GetAsync(2), Times.Once);
        mockBookingRepository.Verify(m => m.RemoveAsync(2), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        mockBookingRepository.Setup(m => m.GetAsync(99)).ReturnsAsync((Booking)null);
        
        // Act
        var result = await controller.Delete(99);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
        mockBookingRepository.Verify(m => m.GetAsync(99), Times.Once);
        mockBookingRepository.Verify(m => m.RemoveAsync(It.IsAny<int>()), Times.Never);
    }
}