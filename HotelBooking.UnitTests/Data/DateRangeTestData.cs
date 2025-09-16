using System;
using System.Collections.Generic;
using Xunit;

namespace HotelBooking.UnitTests.Data;

public class DateRangeTestData : TheoryData<DateTime, DateTime, int>
{
    public DateRangeTestData()
    {
        Add(DateTime.Today.AddDays(1), DateTime.Today.AddDays(5), 0);
        Add(DateTime.Today.AddDays(10), DateTime.Today.AddDays(20), 11);
        Add(DateTime.Today.AddDays(15), DateTime.Today.AddDays(18), 4);
        Add(DateTime.Today.AddDays(25), DateTime.Today.AddDays(30), 0);
    }
}