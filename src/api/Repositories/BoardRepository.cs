using Microsoft.EntityFrameworkCore;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<BoardRepository> _logger;

    public BoardRepository(AppDbContext context, ILogger<BoardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Board>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all boards from the database.");

        return await _context.Boards
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<List<OrderBoardComponent>> GetComponentsAsync(int boardId)
    {
        return await _context.OrderBoardComponents
            .Where(boardComponent => boardComponent.OrderBoard.BoardId  == boardId)
            .Include(boardComponent => boardComponent.Component)
            .ThenInclude(component => component.ComponentType)
            .AsNoTracking()
            .OrderBy(boardComponent => boardComponent.ComponentId)
            .ToListAsync();
    }

    public async Task<Board?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching board with Id {BoardId}.", id);

        return await _context.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Board> AddAsync(Board board)
    {
        _logger.LogInformation("Adding new board named {BoardName}.", board.Name);

        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was created successfully.", board.Id);
        return board;
    }

    public async Task<Board> UpdateAsync(Board board)
    {
        _logger.LogInformation("Updating board {BoardId}.", board.Id);

        _context.Boards.Update(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was updated successfully.", board.Id);
        return board;
    }
    
    public async Task<bool?> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete board {BoardId}.", id);

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board is null)
        {
            _logger.LogWarning("Delete requested for board {BoardId}, but it was not found.", id);
            return null;
        }

        var isInOrder = await _context.OrderBoards
            .AnyAsync(orderBoard => orderBoard.BoardId == id);

        if (isInOrder)
        {
            _logger.LogWarning("Board {BoardId} cannot be deleted because it is referenced by an order.", id);
            return false;
        }

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was deleted successfully.", id);
        return true;
    }
}
