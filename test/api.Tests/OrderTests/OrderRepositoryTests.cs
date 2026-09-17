using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class OrderRepositoryTests
{   
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task SaveAsync_Books_Component_Quantity_And_Marks_Order_Created()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentType = new ComponentType { Id = 1, Name = "Type 1" };
        var component = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType, Quantity = 4 };
        var board = new Board { Id = 1, Name = "Board" };
        context.ComponentTypes.Add(componentType);
        context.Components.Add(component);
        context.Boards.Add(board);
        await context.SaveChangesAsync();
        
        var expectedOrder = new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) };
        var request = new CreateOrderRequest(
            Name: expectedOrder.Name,
            Description: expectedOrder.Description,
            OrderDate: expectedOrder.OrderDate,
            Boards: new List<CreateOrderBoardRequest>
            {
                new CreateOrderBoardRequest(
                    board.Id,
                    Components: new List<CreateOrderBoardComponentRequest>
                    {
                        new CreateOrderBoardComponentRequest(ComponentId: component.Id, Quantity: 4)
                    }
                )
            });

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        var result = await repository.AddAsync(expectedOrder);

        Assert.Equal(OrderStatus.Created, result!.Status);
        Assert.Equal(4, (await context.Components.FindAsync(component.Id))!.Quantity);
    }

    [Fact]
    public async Task Add_Order_With_The_Same_Board_Twice()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var orderFactory = new OrderFactory();
        var componentType1 = new ComponentType { Id = 1, Name = "Type 1" };
        var component1 = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType1, Quantity = 2 };
        var board1 = new Board { Id = 1, Name = "Board 1" };
        
        context.ComponentTypes.AddRange(componentType1);
        context.Components.AddRange(component1);
        context.Boards.AddRange(board1);
        await context.SaveChangesAsync();
        
        var firstOrder = orderFactory.WithName("First Order")
                        .WithBoardAndComponent(board1.Id, component1.Id, 1)
                        .WithBoardAndComponent(board1.Id, component1.Id, 1)
                        .Create();
        
        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        await repository.AddAsync(firstOrder);

        var orderBoards = await context.OrderBoards
            .Include(orderBoard => orderBoard.OrderBoardComponents)
            .Where(orderBoard => orderBoard.OrderId == firstOrder.Id)
            .ToListAsync();

        Assert.Equal(2, orderBoards.Count);
        Assert.All(
            orderBoards.Where(orderBoard => orderBoard.BoardId == board1.Id),
            orderBoard => Assert.Equal(component1.Id, Assert.Single(orderBoard.OrderBoardComponents).ComponentId));
    }

    [Fact]
    public async Task GetComponentsAsync_Uses_OrderBoard_Instance_Not_Board_Id()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentType = new ComponentType { Id = 1, Name = "Type 1" };
        var componentA = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType, Quantity = 5 };
        var componentB = new Component { Id = 2, ComponentTypeId = 1, ComponentType = componentType, Quantity = 5 };
        var board = new Board { Id = 1, Name = "Board 1" };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Desc", OrderDate = DateTime.Today };

        context.ComponentTypes.Add(componentType);
        context.Components.AddRange(componentA, componentB);
        context.Boards.Add(board);
        context.Orders.Add(order);

        var firstOrderBoard = new OrderBoard { Id = Guid.NewGuid(), OrderId = order.Id, BoardId = board.Id, OrderBoardComponents = new List<OrderBoardComponent> { new() { ComponentId = componentA.Id, Quantity = 2 } } };
        var secondOrderBoard = new OrderBoard { Id = Guid.NewGuid(), OrderId = order.Id, BoardId = board.Id, OrderBoardComponents = new List<OrderBoardComponent> { new() { ComponentId = componentB.Id, Quantity = 3 } } };

        context.OrderBoards.AddRange(firstOrderBoard, secondOrderBoard);
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());

        var firstComponents = await repository.GetComponentsAsync(order.Id, firstOrderBoard.Id);
        var secondComponents = await repository.GetComponentsAsync(order.Id, secondOrderBoard.Id);

        Assert.Single(firstComponents);
        Assert.Equal(componentA.Id, firstComponents[0].ComponentId);
        Assert.Equal(2, firstComponents[0].Quantity);

        Assert.Single(secondComponents);
        Assert.Equal(componentB.Id, secondComponents[0].ComponentId);
        Assert.Equal(3, secondComponents[0].Quantity);
    }

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Orders()
    {
        // Arrange
        using var context = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<OrderRepository>>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = Guid.NewGuid(), Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };        
        context.Orders.AddRange(expectedOrders);
        await context.SaveChangesAsync();
        var orderRepository = new OrderRepository(context, fakeLogger);

        // Act
        var result = await orderRepository.GetAllAsync();

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
    public async Task GetExportDataAsync_Returns_Order_With_Boards_And_Components()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentType = new ComponentType { Id = 1, Name = "Type 1" };
        var component = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType, Quantity = 5 };
        var board = new Board { Id = 1, Name = "Board 1", Description = "Board description", Length = 10, Width = 20 };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order 1", OrderDate = new DateTime(2026, 9, 1) };
        var orderBoard = new OrderBoard { OrderId = order.Id, BoardId = board.Id };

        context.ComponentTypes.Add(componentType);
        context.Components.Add(component);
        context.Boards.Add(board);
        context.Orders.Add(order);
        context.OrderBoards.Add(orderBoard);
        context.OrderBoardComponents.Add(new OrderBoardComponent
        {
            OrderBoardId = orderBoard.Id,
            ComponentId = component.Id,
            Quantity = 2
        });
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());

        var result = await repository.GetExportDataAsync(order.Id);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        var exportedBoard = Assert.Single(result.Boards);
        Assert.Equal(board.Name, exportedBoard.Name);
        var exportedComponent = Assert.Single(exportedBoard.Components);
        Assert.Equal(component.Id, exportedComponent.Id);
        Assert.Equal(componentType.Name, exportedComponent.ComponentTypeName);
        Assert.Equal(2, exportedComponent.Quantity);
    }
}