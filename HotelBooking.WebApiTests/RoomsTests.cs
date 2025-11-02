using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.WebApiTests;

public class RoomsTests : IClassFixture<HotelBookingWebApplicationFactory>
{
    private readonly HotelBookingWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RoomsTests(HotelBookingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRooms_ReturnsOkStatus()
    {
        // Arrange
        var request = "/rooms";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var rooms = JsonSerializer.Deserialize<List<Room>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(rooms);
        Assert.NotEmpty(rooms);
    }

    [Fact]
    public async Task GetRooms_ReturnsRoomsWithRequiredProperties()
    {
        // Arrange
        var request = "/rooms";

        // Act
        var response = await _client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var rooms = JsonSerializer.Deserialize<List<Room>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(rooms);
        foreach (var room in rooms)
        {
            Assert.NotNull(room.Description);
            Assert.True(room.Id > 0);
        }
    }

    [Fact]
    public async Task GetRoom_WithValidId_ReturnsRoom()
    {
        // Arrange
        var request = "/rooms/1";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var room = JsonSerializer.Deserialize<Room>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(room);
        Assert.Equal(1, room.Id);
    }

    [Fact]
    public async Task GetRoom_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = "/rooms/9999";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostRoom_WithValidData_ReturnsCreated()
    {
        // Arrange
        var room = new
        {
            description = "Test Room"
        };

        var json = JsonSerializer.Serialize(room);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/rooms", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostRoom_WithEmptyPayload_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/rooms", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var request = "/rooms/1";

        // Act
        var response = await _client.DeleteAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRoom_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var request = "/rooms/0";

        // Act
        var response = await _client.DeleteAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRooms_ReturnsAtLeastThreeRooms()
    {
        // Arrange
        var request = "/rooms";

        // Act
        var response = await _client.GetAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var rooms = JsonSerializer.Deserialize<List<Room>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        Assert.NotNull(rooms);
        Assert.True(rooms.Count >= 3, "Test database should be initialized with at least 3 rooms");
    }
}
