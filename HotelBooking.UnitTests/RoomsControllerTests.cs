using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class RoomsControllerTests
    {
        private RoomsController controller;
        private Mock<IRepository<Room>> fakeRoomRepository;

        public RoomsControllerTests()
        {
            var rooms = new List<Room>
            {
                new() { Id=1, Description="A" },
                new() { Id=2, Description="B" },
            };

            // Create fake RoomRepository. 
            fakeRoomRepository = new Mock<IRepository<Room>>();

            // Implement fake GetAll() method.
            fakeRoomRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(rooms);


            // Implement fake Get() method.
            //fakeRoomRepository.Setup(x => x.GetAsync(2)).ReturnsAsync(rooms[1]);


            // Alternative setup with argument matchers:

            // Any integer:
            //fakeRoomRepository.Setup(x => x.GetAsync(It.IsAny<int>())).ReturnsAsync(rooms[1]);

            // Integers from 1 to 2 (using a predicate)
            // If the fake Get is called with an another argument value than 1 or 2,
            // it returns null, which corresponds to the behavior of the real
            // repository's Get method.
            //fakeRoomRepository.Setup(x => x.GetAsync(It.Is<int>(id => id > 0 && id < 3))).ReturnsAsync(rooms[1]);

            // Integers from 1 to 2 (using a range)
            fakeRoomRepository.Setup(x =>
            x.GetAsync(It.IsInRange<int>(1, 2, Moq.Range.Inclusive))).ReturnsAsync(rooms[1]);


            // Create RoomsController
            controller = new RoomsController(fakeRoomRepository.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsListWithCorrectNumberOfRooms()
        {
            // Act
            var result = await controller.Get() as List<Room>;
            var noOfRooms = result.Count;

            // Assert
            Assert.Equal(2, noOfRooms);
        }

        [Fact]
        public async Task GetById_RoomExists_ReturnsIActionResultWithRoom()
        {
            // Act
            var result = await controller.Get(2) as ObjectResult;
            var room = result.Value as Room;
            var roomId = room.Id;

            // Assert
            Assert.InRange<int>(roomId, 1, 2);
        }
        
        [Fact]
        public async Task GetById_RoomDoesNotExist_ReturnsNotFoundResult()
        {
            // Act
            var result = await controller.Get(3);
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
        
        [Fact]
        public async Task Post_WhenRoomIsNotNull_ReturnsCreatedAtRoute()
        {
            // Arrange
            var room = new Room { Id = 3, Description = "C" };

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
                    x.RemoveAsync(It.Is<int>(id => id < 1 || id > 2))).
                    Throws<InvalidOperationException>();

            Task result() => controller.Delete(3);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(result);

            // Assert against the mock object
            fakeRoomRepository.Verify(x => x.RemoveAsync(It.IsAny<int>()));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(3, false)]
        public async Task GetById_WithVariousIds_ReturnsExpectedResult(int roomId, bool shouldReturnRoom)
        {
            // Act
            var result = await controller.Get(roomId);

            // Assert
            if (shouldReturnRoom)
            {
                Assert.IsType<ObjectResult>(result);
                var objectResult = result as ObjectResult;
                Assert.NotNull(objectResult.Value);
            }
            else
            {
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Theory]
        [MemberData(nameof(RoomTestData))]
        public async Task Post_WithVariousRooms_ReturnsExpectedResult(Room room, Type expectedResultType, bool shouldCallAdd)
        {
            // Act
            var result = await controller.Post(room);

            // Assert
            Assert.IsType(expectedResultType, result);
            if (shouldCallAdd)
            {
                fakeRoomRepository.Verify(x => x.AddAsync(room), Times.Once);
            }
            else
            {
                fakeRoomRepository.Verify(x => x.AddAsync(It.IsAny<Room>()), Times.Never);
            }
        }

        public static IEnumerable<object[]> RoomTestData =>
            new List<object[]>
            {
                new object[] { new Room { Id = 3, Description = "C" }, typeof(CreatedAtRouteResult), true },
                new object[] { new Room { Id = 4, Description = "D" }, typeof(CreatedAtRouteResult), true },
                new object[] { null, typeof(BadRequestResult), false }
            };

        [Theory]
        [ClassData(typeof(RoomValidationTestData))]
        public void Room_Properties_AreSetCorrectly(int id, string description, bool isValid)
        {
            // Arrange & Act
            var room = new Room { Id = id, Description = description };

            // Assert
            if (isValid)
            {
                Assert.True(room.Id > 0);
                Assert.False(string.IsNullOrEmpty(room.Description));
            }
            else
            {
                Assert.True(room.Id <= 0 || string.IsNullOrEmpty(room.Description));
            }
        }
    }

    public class RoomValidationTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { 1, "Room A", true };
            yield return new object[] { 2, "Room B", true };
            yield return new object[] { 0, "Invalid Room", false };
            yield return new object[] { -1, "Negative ID", false };
            yield return new object[] { 1, "", false };
            yield return new object[] { 1, null, false };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
