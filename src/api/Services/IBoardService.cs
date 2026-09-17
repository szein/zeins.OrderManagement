using core.Models;
public interface IBoardService
{
    /// <summary>
    /// Get All Boards
    /// </summary>
    /// <returns></returns>
    Task<List<Board>> GetAllAsync();
    /// <summary>
    /// Get all Components related to the Board with the board Id.
    /// </summary>
    /// <returns></returns>
    Task<List<OrderBoardComponent>> GetComponentsAsync(int boardId);
    /// <summary>
    /// Get Board by its Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Board?> GetByIdAsync(int id);
    /// <summary>
    /// Create Board
    /// </summary>
    /// <returns></returns>
    Task<Board> CreateAsync(string name, string description, double length, double width);
    /// <summary>
    /// Update Board
    /// </summary>
    /// <returns></returns>
    Task<Board?> UpdateAsync(int id, string name, string description, double length, double width);
    /// <summary>
    /// Delete Board by its Id
    /// </summary>
    /// <returns> <strong>false</strong> if not deleted (e.g has relation to an Order), <strong>null</strong> if Board was not found</returns>
    Task<bool?> DeleteAsync(int id);
}
