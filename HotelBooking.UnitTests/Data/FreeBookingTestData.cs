using System;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.UnitTests.Data;

public class FreeBookingTestData : TheoryData<Booking>
{
    public FreeBookingTestData()
    {
        Add(new Booking { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2), CustomerId = 1 });
        Add(
            new Booking
            {
                StartDate = DateTime.Today.AddDays(7), EndDate = DateTime.Today.AddDays(9), CustomerId = 2
            });
        Add(
            new Booking
            {
                StartDate = DateTime.Today.AddDays(21), EndDate = DateTime.Today.AddDays(25), CustomerId = 3
            });
    }
}

public class OccupiedBookingTestData : TheoryData<Booking>
{
    public OccupiedBookingTestData()
    {
        Add(new Booking
            { StartDate = DateTime.Today.AddDays(10), EndDate = DateTime.Today.AddDays(20), CustomerId = 1 });
        Add(
            new Booking
            {
                StartDate = DateTime.Today.AddDays(15), EndDate = DateTime.Today.AddDays(17), CustomerId = 2
            });
    }
}