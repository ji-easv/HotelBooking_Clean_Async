using System;
using System.Net;
using System.Threading.Tasks;
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
    private readonly Mock<IRepository<Booking>> mockBookingRepository = new();
    private readonly Mock<IBookingManager> mockBookingManager = new();
    private readonly BookingsController controller;

    private IActionResult lastResult;
    private Booking bookingRequest;
    private int bookingId;

    public BookingStepDefinitions()
    {
        controller = new BookingsController(mockBookingRepository.Object, mockBookingManager.Object);
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

    [Given("a customer provides incomplete booking information")]
    public void GivenACustomerProvidesIncompleteBookingInformation()
    {
        bookingRequest = null;
    }

    [When("the customer submits the booking request")]
    public async Task WhenTheCustomerSubmitsTheBookingRequest()
    {
        lastResult = await controller.Post(bookingRequest);
    }

    [Then("the customer should receive a validation error message")]
    public void ThenTheCustomerShouldReceiveAValidationErrorMessage()
    {
        Assert.IsType<BadRequestResult>(lastResult);
    }

    [Given("a booking exists with ID {int}")]
    public void GivenABookingExistsWithID(int id)
    {
        bookingId = id;
        mockBookingRepository.Setup(m => m.GetAsync(id)).ReturnsAsync(new Booking
        {
            Id = id, CustomerId = 1, RoomId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1),
            IsActive = true
        });
    }

    [When("the customer requests the booking details for ID {int}")]
    public async Task WhenTheCustomerRequestsTheBookingDetailsForID(int id)
    {
        lastResult = await controller.Get(id);
    }

    [Then("the booking details should be returned")]
    public void ThenTheBookingDetailsShouldBeReturned()
    {
        Assert.IsType<ObjectResult>(lastResult);
        var obj = (lastResult as ObjectResult)?.Value as Booking;
        Assert.NotNull(obj);
        Assert.Equal(bookingId, obj.Id);
    }

    [When("the customer cancels the booking with ID {int}")]
    public async Task WhenTheCustomerCancelsTheBookingWithID(int id)
    {
        mockBookingRepository.Setup(m => m.RemoveAsync(id)).Returns(Task.CompletedTask);
        lastResult = await controller.Delete(id);
    }

    [Then("the booking should be removed from the system")]
    public void ThenTheBookingShouldBeRemovedFromTheSystem()
    {
        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(lastResult);
        mockBookingRepository.Verify(m => m.RemoveAsync(bookingId), Moq.Times.Once);
    }

    [Then("the room should become available for those dates")]
    public void ThenTheRoomShouldBecomeAvailableForThoseDates()
    {
        // This would be handled by business logic; for BDD, we verify RemoveAsync was called
        mockBookingRepository.Verify(m => m.RemoveAsync(bookingId), Moq.Times.Once);
    }

    [Given("all rooms are already booked for the requested dates")]
    public void GivenAllRoomsAreAlreadyBookedForTheRequestedDates()
    {
        mockBookingManager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(false);
    }
}