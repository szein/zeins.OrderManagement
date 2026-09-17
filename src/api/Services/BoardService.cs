#nullable disable
using core.Models;

public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<BoardService> _logger;

    public BoardService(IBoardRepository boardRepository, ILogger<BoardService> logger)
    {
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<List<Board>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all boards.");
        return await _boardRepository.GetAllAsync();
    }

    public async Task<List<OrderBoardComponent>> GetComponentsAsync(int boardId)
    {
        return await _boardRepository.GetComponentsAsync(boardId);
    }

    public async Task<Board> GetByIdAsync(int id)
    {
        _logger.LogInformation("Service request received to fetch board {BoardId}.", id);
        return await _boardRepository.GetByIdAsync(id);
    }

    public async Task<Board> CreateAsync(string name, string description, double length, double width)
    {
        _logger.LogInformation("Creating a new board with name {BoardName}.", name);

        var board = new Board
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Length = length,
            Width = width
        };

        var createdBoard = await _boardRepository.AddAsync(board);
        _logger.LogInformation("Board creation completed for board {BoardId}.", createdBoard.Id);
        return createdBoard;
    }

    public async Task<Board> UpdateAsync(int id, string name, string description, double length, double width)
    {
        _logger.LogInformation("Updating board {BoardId}.", id);

        var existingBoard = await _boardRepository.GetByIdAsync(id);
        if (existingBoard is null)
        {
            _logger.LogWarning("Update requested for missing board {BoardId}.", id);
            return null;
        }

        existingBoard.Name = name.Trim();
        existingBoard.Description = description.Trim();
        existingBoard.Length = length;
        existingBoard.Width = width;

        var updatedBoard = await _boardRepository.UpdateAsync(existingBoard);
        _logger.LogInformation("Board {BoardId} was successfully updated.", updatedBoard.Id);
        return updatedBoard;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete request received for board {id}.", id);
        var deleted = await _boardRepository.DeleteAsync(id);
        if (deleted is null)
        {
            _logger.LogWarning("Delete operation failed because board {id} was not found.", id);
        }

        if (deleted.Value)
        {
            _logger.LogWarning("Board {id} cannot be deleted because it is referenced by an order", id);
        }

        return deleted;
    }
}
