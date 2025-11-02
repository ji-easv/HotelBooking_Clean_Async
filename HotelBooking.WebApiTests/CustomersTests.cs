using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.WebApiTests;

public class CustomersTests : IClassFixture<HotelBookingWebApplicationFactory>
{
    private readonly HotelBookingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CustomersTests(HotelBookingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ReturnsOkStatus()
    {
        // Arrange
        var request = "/customers";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(customers);
        Assert.NotEmpty(customers);
    }

    [Fact]
    public async Task GetCustomers_ReturnsCustomersWithRequiredProperties()
    {
        // Arrange
        var request = "/customers";

        // Act
        var response = await _client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(customers);
        foreach (var customer in customers)
        {
            Assert.NotNull(customer.Name);
            Assert.NotNull(customer.Email);
            Assert.True(customer.Id > 0);
        }
    }

    [Fact]
    public async Task GetCustomers_ReturnsAtLeastTwoCustomers()
    {
        // Arrange
        var request = "/customers";

        // Act
        var response = await _client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<List<Customer>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(customers);
        Assert.True(customers.Count >= 2, "Test database should be initialized with at least 2 customers");
    }
}
