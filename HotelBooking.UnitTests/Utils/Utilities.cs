using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;

namespace HotelBooking.UnitTests.Utils;

public static class Utilities
{
    private static readonly DateTime FullyOccupiedStartDate = DateTime.Today.AddDays(10);
    private static readonly DateTime FullyOccupiedEndDate = DateTime.Today.AddDays(20);
    
    public static Mock<IRepository<Booking>> SetUpBookingRepositoryMocks()
    {
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2), IsActive = true,
                CustomerId = 1, RoomId = 1
            },
            new()
            {
                Id = 2, StartDate = FullyOccupiedStartDate, EndDate = FullyOccupiedEndDate, IsActive = true,
                CustomerId = 1, RoomId = 1
            },
            new()
            {
                Id = 3, StartDate = FullyOccupiedStartDate, EndDate = FullyOccupiedEndDate, IsActive = true,
                CustomerId = 2, RoomId = 2
            },
        };

        var bookingRepository = new Mock<IRepository<Booking>>();
        bookingRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(bookings);
        bookingRepository.Setup(x => x.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);
        bookingRepository.Setup(x => x.GetAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => bookings.FirstOrDefault(b => b.Id == id));
        
        
        return bookingRepository;
    }

    public static Mock<IRepository<Room>> SetUpRoomRepositoryMocks()
    {
        var roomRepository = new Mock<IRepository<Room>>();

        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "A" },
            new() { Id = 2, Description = "B" },
        };
        roomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);
        return roomRepository;
    }
}