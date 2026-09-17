using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;

public class OrderServiceTests
{
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task DeleteAsync_Does_Not_Delete_Created_Order()
    {
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Created };
        var repository = A.Fake<IOrderRepository>();
        A.CallTo(() => repository.GetByIdAsync(order.Id)).Returns(order);
        var service = new OrderService(repository, A.Fake<ILogger<OrderService>>());

        var result = await service.DeleteAsync(order.Id);
        Assert.False(result,"Order was delete when it must not!");
    }

    [Fact]
    public async Task Get_All_Orders_Returns_List_Of_Orders()
    {
        // Arrange
        var fakeLogger = A.Fake<ILogger<OrderService>>();
        var fakeRepository = A.Fake<IOrderRepository>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = Guid.NewGuid(), Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };
        A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult(expectedOrders));
        var orderService = new OrderService(fakeRepository, fakeLogger);

        // Act
        var result = await orderService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<Order>>(result);
        Assert.Equal(expectedOrders.Count, result.Count);
        foreach (var order in expectedOrders)
        {
            Assert.Contains(result, o => o.Id == order.Id && o.Name == order.Name && o.Description == order.Description && o.OrderDate == order.OrderDate);
        }
    }

    [Fact]
    public async Task CreateOrder_Books_Quantity_From_Component()
    {
        // Arrange
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var orderRepository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        var expectedBoard = new Board() { Name = "Board 1", Length = 1.0, Width = 1.0 };
        var expectedComponentType = new ComponentType { Name = "Component 1" };
        var expectedComponent = new Component { Id = 1, ComponentTypeId = expectedComponentType.Id, Quantity = 5 };
        context.ComponentTypes.Add(expectedComponentType);
        context.Components.Add(expectedComponent);
        context.Boards.Add(expectedBoard);
        await context.SaveChangesAsync();

        var expectedOrder = new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) };
        var request = new CreateOrderRequest(
            Name: expectedOrder.Name,
            Description: expectedOrder.Description,
            OrderDate: expectedOrder.OrderDate,
            Boards: new List<CreateOrderBoardRequest>
            {
                new CreateOrderBoardRequest(
                    expectedBoard.Id,
                    Components: new List<CreateOrderBoardComponentRequest>
                    {
                        new CreateOrderBoardComponentRequest(ComponentId: expectedComponent.Id, Quantity: 4)
                    }
                )
            });


        //Action
        var orderService = new OrderService(orderRepository, A.Fake<ILogger<OrderService>>());
        var result = await orderService.CreateAsync(request);

        //Assert
        var actualQuantity = context.Components.First(c => c.Id == expectedComponent.Id).Quantity;
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.Name, result.Name);
        Assert.Equal(expectedOrder.Description, result.Description);
        Assert.Equal(expectedOrder.OrderDate, result.OrderDate);
        Assert.Equal(1, actualQuantity);
        //Assert.Equal(OrderStatus.Pending, result.Status);
    }

}