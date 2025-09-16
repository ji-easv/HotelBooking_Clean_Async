using System;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.UnitTests.Data;

public class BookingTestData : TheoryData<Booking, bool>
{
    public BookingTestData()
    {
        Add(new Booking { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2), CustomerId = 1 },
            true);
        Add(
            new Booking
            {
                StartDate = DateTime.Today.AddDays(10), EndDate = DateTime.Today.AddDays(15), CustomerId = 2
            }, false);
        Add(
            new Booking
            {
                StartDate = DateTime.Today.AddDays(21), EndDate = DateTime.Today.AddDays(25), CustomerId = 3
            }, true);
    }
}

