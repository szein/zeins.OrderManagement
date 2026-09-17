public interface IBoardRepository
{   
    /// <summary>
    /// Gets all Boards
    /// </summary>
    /// <returns></returns>
    Task<List<Board>> GetAllAsync();
    /// <summary>Gets all components for a board.
    /// </summary>
    /// <returns></returns>
    Task<List<OrderBoardComponent>> GetComponentsAsync(int boardId);
    /// <summary>Gets a board by its ID.
    /// </summary>
    /// <returns></returns>
    Task<Board?> GetByIdAsync(int id);
    /// <summary>Adds a new board.
    /// </summary>
    /// <returns></returns>
    Task<Board> AddAsync(Board board);    
    /// <summary>Updates an existing board.
    /// </summary>
    /// <returns></returns>
    Task<Board> UpdateAsync(Board board);
    /// <summary>Deletes a board by its ID.
    /// </summary>
    /// <returns></returns>
    Task<bool?> DeleteAsync(int id);
}
