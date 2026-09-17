using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;

public class BoardServiceTests
{
    [Fact]
    public async Task Get_All_Boards_Returns_List_Of_Boards()
    {
        var fakeLogger = A.Fake<ILogger<BoardService>>();
        var fakeRepository = A.Fake<IBoardRepository>();
        var expectedBoards = new List<Board>
        {
            new Board { Id = 1, Name = "Board 1", Description = "Description 1", Length = 10.5, Width = 8.5 },
            new Board { Id = 2, Name = "Board 2", Description = "Description 2", Length = 12.0, Width = 9.0 }
        };

        A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult(expectedBoards));

        var boardService = new BoardService(fakeRepository, fakeLogger);

        var result = await boardService.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Board>>(result);
        Assert.Equal(expectedBoards.Count, result.Count);
        foreach (var board in expectedBoards)
        {
            Assert.Contains(result, b => b.Id == board.Id && b.Name == board.Name && b.Description == board.Description && b.Length == board.Length && b.Width == board.Width);
        }
    }

}
