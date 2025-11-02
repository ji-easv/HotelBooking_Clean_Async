using System;
using System.Linq;
using HotelBooking.Infrastructure;
using HotelBooking.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.WebApiTests;

public class HotelBookingWebApplicationFactory : WebApplicationFactory<TestEntryPoint>
{
    private readonly string _dbName;

    public HotelBookingWebApplicationFactory()
    {
        // Create a unique database name for each factory instance
        _dbName = $"HotelBookingTestDb_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the default DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<HotelBookingContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Register a fresh in-memory database for this factory instance
            services.AddDbContext<HotelBookingContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.UseEnvironment("Test");
    }
}
