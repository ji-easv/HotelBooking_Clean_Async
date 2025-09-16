using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.WebApi.Controllers;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests;

public class CustomersControllerTests
{
    private CustomersController controller;
    private Mock<IRepository<Customer>> mockCustomerRepository;
    
    public CustomersControllerTests()
    {
        var customers = new List<Customer>
        {
            new() { Id=1, Name="Alice" },
            new() { Id=2, Name="Bob" },
        };

        // Create mock CustomerRepository. 
        mockCustomerRepository = new Mock<IRepository<Customer>>();

        // Implement mock GetAll() method.
        mockCustomerRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customers);

        // Implement mock Get() method.
        mockCustomerRepository.Setup(x =>
        x.GetAsync(It.IsInRange<int>(1, 2, Moq.Range.Inclusive))).ReturnsAsync(customers[1]);

        // Create CustomersController
        controller = new CustomersController(mockCustomerRepository.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsListWithCorrectNumberOfCustomers()
    {
        // Act
        var result = await controller.Get() as List<Customer>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}