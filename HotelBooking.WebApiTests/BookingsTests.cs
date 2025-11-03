using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.WebApiTests;

public class BookingsTests : IClassFixture<HotelBookingWebApplicationFactory>
{
    private readonly HotelBookingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BookingsTests(HotelBookingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBookings_ReturnsOkStatus()
    {
        // Arrange
        var request = "/bookings";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var bookings = JsonSerializer.Deserialize<List<Booking>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(bookings);
        Assert.NotEmpty(bookings);
    }

    [Fact]
    public async Task GetBooking_WithValidId_ReturnsBooking()
    {
        // Arrange
        var request = "/bookings/1";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var booking = JsonSerializer.Deserialize<Booking>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(booking);
        Assert.Equal(1, booking.Id);
    }

    [Fact]
    public async Task GetBooking_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = "/bookings/9999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 9, 201)] // Before occupied range
    [InlineData(21, 22, 201)] // After occupied range
    [InlineData(9, 21, 409)]
    [InlineData(9, 10, 409)]
    [InlineData(9, 20, 409)]
    [InlineData(10, 21, 409)]
    [InlineData(20, 21, 409)]
    [InlineData(10, 20, 409)]
    [InlineData(10, 10, 409)]
    [InlineData(20, 20, 409)]
    public async Task PostBooking_WithVariousDateRanges_ReturnsExpectedStatus(int startOffset, int endOffset, int expectedStatus)
    {
        // Arrange
        var booking = new
        {
            startDate = DateTime.Today.AddDays(startOffset),
            endDate = DateTime.Today.AddDays(endOffset),
            isActive = true,
            customerId = 1,
            roomId = 1
        };

        var json = JsonSerializer.Serialize(booking);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/bookings", content);

        // Assert
        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task PostBooking_WithEmptyPayload_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/bookings", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutBooking_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var booking = new
        {
            id = 1,
            startDate = DateTime.Today.AddDays(10),
            endDate = DateTime.Today.AddDays(20),
            isActive = false,
            customerId = 2,
            roomId = 1
        };

        var json = JsonSerializer.Serialize(booking);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/bookings/1", content);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PutBooking_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var booking = new
        {
            id = 999,
            startDate = DateTime.Today.AddDays(10),
            endDate = DateTime.Today.AddDays(20),
            isActive = true,
            customerId = 1,
            roomId = 1
        };

        var json = JsonSerializer.Serialize(booking);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/bookings/1", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBooking_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var request = "/bookings/1";

        // Act
        var response = await _client.DeleteAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBooking_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = "/bookings/9999";

        // Act
        var response = await _client.DeleteAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
