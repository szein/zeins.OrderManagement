using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


public class ComponentServiceTests
{
    private DbContextFactory _dbContextFactory = new DbContextFactory();    

    [Fact]
    public async Task Create_Component_Creates_Component_And_ComponentType()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentRepository = new ComponentRepository(context, A.Fake<ILogger<ComponentRepository>>());
        var componentTypeRepository = new ComponentTypeRepository(context, A.Fake<ILogger<ComponentTypeRepository>>());
        var service = new ComponentService(componentRepository, componentTypeRepository, A.Fake<ILogger<ComponentService>>());

        var component = await service.CreateAsync("Type 1", "Description", 10);

        Assert.Equal(10, component.Quantity);
        Assert.Equal("Type 1", (await context.ComponentTypes.SingleAsync()).Name);
        Assert.Equal(component.ComponentTypeId, (await context.ComponentTypes.SingleAsync()).Id);
        Assert.Equal(component.ComponentTypeId, (await context.Components.SingleAsync()).ComponentTypeId);
    }

    [Fact]
    public async Task Update_Component_Updates_Component_And_Existing_ComponentType()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        context.ComponentTypes.Add(new ComponentType { Id = 1, Name = "Type 1", Description = "Description" });
        context.Components.Add(new Component { Id = 1, ComponentTypeId = 1, Quantity = 10 });
        await context.SaveChangesAsync();

        var componentRepository = new ComponentRepository(context, A.Fake<ILogger<ComponentRepository>>());
        var componentTypeRepository = new ComponentTypeRepository(context, A.Fake<ILogger<ComponentTypeRepository>>());
        var service = new ComponentService(
            componentRepository,
            componentTypeRepository,
            A.Fake<ILogger<ComponentService>>());

        var updatedComponent = await service.UpdateAsync(1, "Updated Type", "Updated description", 20);

        Assert.NotNull(updatedComponent);
        Assert.Equal(20, updatedComponent.Quantity);
        Assert.Equal(20, (await context.Components.FindAsync(1))!.Quantity);
        Assert.Equal("Updated Type", (await context.ComponentTypes.FindAsync(1))!.Name);
    }

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Components()
    {
        using var fakeAppContext =  _dbContextFactory.CreateFakeDbContext();
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
        
        fakeAppContext.ComponentTypes.AddRange(expectedComponentTypes);
        fakeAppContext.Components.AddRange(expectedComponents);
        await fakeAppContext.SaveChangesAsync();

        var componentRepository = new ComponentRepository(fakeAppContext, fakeLogger);

        var result = await componentRepository.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Component>>(result);
        Assert.Equal(expectedComponents.Count, result.Count);
        foreach (var component in expectedComponents)
        {
            Assert.Contains(result, c => c.Id == component.Id && c.ComponentTypeId == component.ComponentTypeId);
        }
    }
    
}