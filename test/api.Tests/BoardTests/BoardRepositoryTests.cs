using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class BoardRepositoryTests
{
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task DeleteAsync_Does_Not_Delete_Board_Referenced_By_Order()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var board = new Board { Id = 1, Name = "Board", Description = "Description" };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order" };
        context.Boards.Add(board);
        context.Orders.Add(order);
        context.OrderBoards.Add(new OrderBoard { OrderId = order.Id, BoardId = board.Id });
        await context.SaveChangesAsync();

        var repository = new BoardRepository(context, A.Fake<ILogger<BoardRepository>>());

        Assert.False(await repository.DeleteAsync(board.Id));
        Assert.NotNull(await context.Boards.SingleOrDefaultAsync(b => b.Id == board.Id));
    }

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Boards()
    {
        using var fakeAppContext = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<BoardRepository>>();
        var expectedBoards = new List<Board>
        {
            new Board { Id = 1, Name = "Board 1", Description = "Description 1", Length = 10.5, Width = 8.5 },
            new Board { Id = 2, Name = "Board 2", Description = "Description 2", Length = 12.0, Width = 9.0 }
        };

        fakeAppContext.Boards.AddRange(expectedBoards);
        await fakeAppContext.SaveChangesAsync();

        //A.CallTo(() => fakeAppContext.Boards.ToListAsync<Board>()).Returns(Task.FromResult(expectedBoards));

        var boardRepository = new BoardRepository(fakeAppContext, fakeLogger);

        var result = await boardRepository.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Board>>(result);
        Assert.Equal(expectedBoards.Count, result.Count);
        foreach (var board in expectedBoards)
        {
            Assert.Contains(result, b => b.Id == board.Id && b.Name == board.Name && b.Description == board.Description && b.Length == board.Length && b.Width == board.Width);
        }
    }
}
