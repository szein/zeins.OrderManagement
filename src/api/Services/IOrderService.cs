using core.Models;
public interface IOrderService
{
    /// <summary>
    /// Get all Orders
    /// </summary>
    /// <returns>List of Orders</returns>
    Task<List<Order>> GetAllAsync();
    /// <summary>
    /// Get board occurrences in an Order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns>List of board occurrences</returns>
    Task<List<OrderBoardRequest>> GetBoardsAsync(Guid orderId);
    /// <summary>
    /// Get all components for a specific board occurrence in the order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="orderBoardId"></param>
    /// <returns>Components assigned to the board instance within the order</returns>
    Task<List<OrderBoardComponent>> GetComponentsAsync(Guid orderId, Guid orderBoardId);
    /// <summary>
    /// Get Order by its Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Order?> GetByIdAsync(Guid id);
    /// <summary>
    /// Get order realted data from repository 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<OrderExportResponse?> GetExportDataAsync(Guid id);
    /// <summary>
    /// Create new Order with its Boards and Components
    /// </summary>
    /// <param name="request">A DTO that holds the order, boards and componants data</param>
    /// <returns></returns>
    Task<Order> CreateAsync(CreateOrderRequest request);
    /// <summary>
    /// Update the order data (name, description and order data)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    Task<Order?> UpdateAsync(Guid id, string name, string description, DateTime orderDate);
    /// <summary>
    /// Check if order can be deleted
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool?> CanDeleteAsync(Guid id);
    /// <summary>
    /// Delete Order by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id);
}
