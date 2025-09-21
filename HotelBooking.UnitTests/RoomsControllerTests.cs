using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class RoomsControllerTests
{
    private RoomsController controller;
    private Mock<IRepository<Room>> fakeRoomRepository;

    public RoomsControllerTests()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Description = "A" },
            new() { Id = 2, Description = "B" },
        };

        // Create fake RoomRepository. 
        fakeRoomRepository = new Mock<IRepository<Room>>();

        // Implement fake GetAll() method.
        fakeRoomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);


        // Implement fake Get() method.
        fakeRoomRepository.Setup(x => x.GetAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => rooms.FirstOrDefault(r => r.Id == id));


        // Create RoomsController
        controller = new RoomsController(fakeRoomRepository.Object);
    }


    public static IEnumerable<object[]> RoomTestData =>
        new List<object[]>
        {
            new object[] { new Room { Id = 3, Description = "C" } },
            new object[] { new Room { Id = 4, Description = "D" } },
        };

    [Fact]
    public async Task GetAll_ReturnsListWithCorrectNumberOfRooms()
    {
        // Act
        var result = await controller.Get() as List<Room>;
        var noOfRooms = result.Count;

        // Assert
        Assert.Equal(2, noOfRooms);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetById_RoomExists_ReturnsIActionResultWithRoom(int roomId)
    {
        // Act
        var result = await controller.Get(roomId) as ObjectResult;
        var resultValue = result?.Value as Room;

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(resultValue);
        Assert.Equal(roomId, resultValue.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task GetById_RoomDoesNotExist_ReturnsNotFoundResult(int roomId)
    {
        // Act
        var result = await controller.Get(roomId);
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_WhenRoomIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await controller.Post(null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Theory]
    [MemberData(nameof(RoomTestData))]
    public async Task Post_WhenRoomIsNotNull_ReturnsCreatedAtRoute(Room room)
    {
        // Act
        var result = await controller.Post(room);

        // Assert against the mock object
        fakeRoomRepository.Verify(x => x.AddAsync(room), Times.Once);
        Assert.IsType<CreatedAtRouteResult>(result);
    }

    [Fact]
    public async Task Delete_WhenIdIsLargerThanZero_RemoveIsCalled()
    {
        // Act
        await controller.Delete(1);

        // Assert against the mock object
        fakeRoomRepository.Verify(x => x.RemoveAsync(1), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenIdIsLessThanOne_RemoveIsNotCalled()
    {
        // Act
        await controller.Delete(0);

        // Assert against the mock object
        fakeRoomRepository.Verify(x => x.RemoveAsync(It.IsAny<int>()), Times.Never());
    }

    [Fact]
    public async Task Delete_WhenIdIsLargerThanTwo_RemoveThrowsException()
    {
        // Instruct the fake Remove method to throw an InvalidOperationException, if a room id that
        // does not exist in the repository is passed as a parameter. This behavior corresponds to
        // the behavior of the real repoository's Remove method.
        fakeRoomRepository.Setup(x =>
            x.RemoveAsync(It.Is<int>(id => id < 1 || id > 2))).Throws<InvalidOperationException>();

        Task result() => controller.Delete(3);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(result);

        // Assert against the mock object
        fakeRoomRepository.Verify(x => x.RemoveAsync(It.IsAny<int>()));
    }
}