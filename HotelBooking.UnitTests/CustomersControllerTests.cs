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


    [Theory]
    [MemberData(nameof(CustomerTestData))]
    public void Customer_Properties_AreSetCorrectly(int id, string name, bool isValidCustomer)
    {
        // Arrange & Act
        var customer = new Customer { Id = id, Name = name };

        // Assert
        if (isValidCustomer)
        {
            Assert.True(customer.Id > 0);
            Assert.False(string.IsNullOrEmpty(customer.Name));
        }
        else
        {
            Assert.True(customer.Id <= 0 || string.IsNullOrEmpty(customer.Name));
        }
    }

    public static IEnumerable<object[]> CustomerTestData =>
        new List<object[]>
        {
            new object[] { 1, "Alice", true },
            new object[] { 2, "Bob", true },
            new object[] { 0, "Invalid", false },
            new object[] { -1, "NegativeId", false },
            new object[] { 1, "", false },
            new object[] { 1, null, false }
        };
}