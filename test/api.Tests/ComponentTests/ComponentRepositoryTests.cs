using Xunit;
using core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FakeItEasy;

public class ComponentRepositoryTests
{
    private DbContextFactory _dbContextFactory = new DbContextFactory();


    [Fact]
    public async Task DeleteAsync_Does_Not_Delete_Ordered_Component_And_Removes_Uncommitted_Component_And_Type()
    {
        await using var context = new DbContextFactory().CreateFakeDbContext();
        var orderedType = new ComponentType { Id = 1, Name = "Ordered", Description = "Ordered component" };
        var removableType = new ComponentType { Id = 2, Name = "Removable", Description = "Removable component" };
        var orderedComponent = new Component { Id = 1, ComponentTypeId = 1, Quantity = 10, ComponentType = orderedType };
        var removableComponent = new Component { Id = 2, ComponentTypeId = 2, Quantity = 10, ComponentType = removableType };
        var board = new Board { Id = 1, Name = "Board", Description = "Board" };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order", OrderDate = DateTime.UtcNow };
        var expectedOrderBoard = new OrderBoard { OrderId = order.Id, BoardId = board.Id };

        context.ComponentTypes.AddRange(orderedType, removableType);
        context.Components.AddRange(orderedComponent, removableComponent);
        context.Boards.Add(board);
        context.Orders.Add(order);
        context.OrderBoards.Add(expectedOrderBoard);
        context.OrderBoardComponents.Add(new OrderBoardComponent { OrderBoardId = expectedOrderBoard.Id, ComponentId = orderedComponent.Id, Quantity = 1 });
        await context.SaveChangesAsync();

        var repository = new ComponentRepository(context, A.Fake<ILogger<ComponentRepository>>());

        Assert.False(await repository.DeleteAsync(orderedComponent.Id));
        Assert.True(await repository.DeleteAsync(removableComponent.Id));

        var ordered = await context.Components.SingleOrDefaultAsync(c => c.Id == orderedComponent.Id);
        Assert.NotNull(ordered);
        Assert.Equal(10, ordered.Quantity);
        Assert.NotEqual(ComponentStatus.Abandoned, ordered.Status);
        Assert.Null(await context.Components.SingleOrDefaultAsync(c => c.Id == removableComponent.Id));
        Assert.Null(await context.ComponentTypes.SingleOrDefaultAsync(ct => ct.Id == removableType.Id));
    }
    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Components()
    {
        //Arrenge
        using var fakeAppContext = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<ComponentRepository>>();
        var expectedComponentTypes = new List<ComponentType>
        {
            new ComponentType { Id = 1, Name = "Component Type 1", Description = "Description 1" },
            new ComponentType { Id = 2, Name = "Component Type 2", Description = "Description 2" }
        };
        var expectedComponents = new List<Component>
        {
            new Component { Id = 1, ComponentTypeId = expectedComponentTypes[0].Id, Quantity = 100, ComponentType = expectedComponentTypes[0]},
            new Component { Id = 2, ComponentTypeId = expectedComponentTypes[1].Id, Quantity = 200, ComponentType = expectedComponentTypes[1]}
        };

        fakeAppContext.Components.AddRange(expectedComponents);
        await fakeAppContext.SaveChangesAsync();

        var componentRepository = new ComponentRepository(fakeAppContext, fakeLogger);

        //Act
        var result = await componentRepository.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Component>>(result);
        Assert.Equal(expectedComponents.Count, result.Count);
    }

    [Fact]
    public async Task GetAvailableQuantityAsync_Returns_Correct_Quantity()
    {
        // Arrange
        using var fakeAppContext = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<ComponentRepository>>();
        var componentTypeId = 1;
        var expectedQuantity = 50;

        fakeAppContext.Components.Add(new Component { Id = 1, ComponentTypeId = componentTypeId, Quantity = expectedQuantity });
        await fakeAppContext.SaveChangesAsync();

        var componentRepository = new ComponentRepository(fakeAppContext, fakeLogger);

        // Act
        var result = await componentRepository.GetAvailableQuantityAsync(componentTypeId);

        // Assert
        Assert.Equal(expectedQuantity, result);
    }
}