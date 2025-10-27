using System;
using System.Collections.Generic;
using System.Linq;
using HotelBooking.Core;

namespace HotelBooking.Infrastructure
{
    public class TestDbInitializer : IDbInitializer
    {
        public void Initialize(HotelBookingContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (context.Booking.Any())
            {
                return; // DB has been seeded
            }

            var customers = new List<Customer>
            {
                new() { Name = "Test User 1", Email = "test1@example.com" },
                new() { Name = "Test User 2", Email = "test2@example.com" }
            };

            var rooms = new List<Room>
            {
                new() { Description = "Room 1" },
                new() { Description = "Room 2" },
                new() { Description = "Room 3" }
            };

            context.Customer.AddRange(customers);
            context.Room.AddRange(rooms);
            context.SaveChanges();

            Console.WriteLine("Seeding bookings...");
            var startDate = DateTime.Today.AddDays(10);
            var endDate = DateTime.Today.AddDays(20);
            var roomList = context.Room.ToList();
            var bookings = roomList.Select((room, idx) => new Booking
            {
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                CustomerId = customers[(idx % customers.Count)].Id,
                RoomId = room.Id
            }).ToList();

            context.Booking.AddRange(bookings);
            context.SaveChanges();
        }
    }
}
