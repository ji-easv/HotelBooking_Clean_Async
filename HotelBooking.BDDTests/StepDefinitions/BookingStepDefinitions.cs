using System;
using System.Net;
using System.Threading.Tasks;
using HotelBooking.BDDTests.Utils;
using HotelBooking.Core;
using HotelBooking.Core.Interfaces;
using HotelBooking.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Reqnroll;
using Xunit;

namespace HotelBooking.BDDTests.StepDefinitions;

[Binding]
public sealed class BookingStepDefinitions
{
    private readonly BookingsController controller;
    private readonly Mock<IBookingManager> mockBookingManager = new();

    private IActionResult lastResult;
    private Booking bookingRequest;

    public BookingStepDefinitions()
    {
        var mockBookingRepository1 = Utilities.SetUpBookingRepositoryMocks();
        controller = new BookingsController(mockBookingRepository1.Object, mockBookingManager.Object);
    }

    [Given("there is at least one available room")]
    public void GivenThereIsAtLeastOneAvailableRoom()
    {
        mockBookingManager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(true);
    }

    [Given(@"a customer wants to book a room from {int} days from today to {int} days from today")]
    public void GivenACustomerWantsToBookARoomFromTo(int startDateOffset, int endDateOffset)
    {
        bookingRequest = new Booking
        {
            CustomerId = 1,
            StartDate = DateTime.Now.AddDays(startDateOffset),
            EndDate = DateTime.Now.AddDays(endDateOffset),
            RoomId = 1,
            IsActive = true
        };
    }

    [When("the customer submits a booking request")]
    public async Task WhenTheCustomerSubmitsABookingRequest()
    {
        lastResult = await controller.Post(bookingRequest);
    }

    [Then("the booking should be created successfully")]
    public void ThenTheBookingShouldBeCreatedSuccessfully()
    {
        Assert.IsType<CreatedAtRouteResult>(lastResult);
        mockBookingManager.Verify(m => m.CreateBooking(It.IsAny<Booking>()), Moq.Times.Once);
    }

    [Then("the room should be marked as booked for those dates")]
    public void ThenTheRoomShouldBeMarkedAsBookedForThoseDates()
    {
        mockBookingManager.Verify(
            m => m.CreateBooking(It.Is<Booking>(b =>
                b.StartDate == bookingRequest.StartDate && b.EndDate == bookingRequest.EndDate)), Moq.Times.Once);
    }

    [Then("the booking should be rejected")]
    public void ThenTheBookingShouldBeRejected()
    {
        Assert.IsType<ConflictObjectResult>(lastResult);
    }

    [Then("the customer should receive a message that no rooms are available")]
    public void ThenTheCustomerShouldReceiveAMessageThatNoRoomsAreAvailable()
    {
        var conflict = lastResult as ConflictObjectResult;
        Assert.NotNull(conflict);
        Assert.Equal(conflict.StatusCode, (int)HttpStatusCode.Conflict);
        Assert.Equal(conflict.Value, "The booking could not be created. All rooms are occupied. Please try another period.");
    }

    [When("the customer submits the booking request")]
    public async Task WhenTheCustomerSubmitsTheBookingRequest()
    {
        lastResult = await controller.Post(bookingRequest);
    }

    [Given("all rooms are already booked for the requested dates")]
    public void GivenAllRoomsAreAlreadyBookedForTheRequestedDates()
    {
        mockBookingManager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(false);
    }
}