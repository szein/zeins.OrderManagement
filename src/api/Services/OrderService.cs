using core.Models;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all orders.");
        return await _orderRepository.GetAllAsync();
    }

    public async Task<List<OrderBoardRequest>> GetBoardsAsync(Guid orderId)
    {
        return await _orderRepository.GetBoardsAsync(orderId);
    }

    public async Task<List<OrderBoardComponent>> GetComponentsAsync(Guid orderId, Guid orderBoardId)
    {
        return await _orderRepository.GetComponentsAsync(orderId, orderBoardId);
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Service request received to fetch order {OrderId}.", id);
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<OrderExportResponse?> GetExportDataAsync(Guid id)
    {
        _logger.LogInformation("Service request received to fetch export data for order {OrderId}.", id);
        return await _orderRepository.GetExportDataAsync(id);
    }

    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating a new order with name {OrderName} and date {OrderDate}.", request.Name, request.OrderDate);

        var order = new Order {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            OrderDate = request.OrderDate,
            OrderBoards = request.Boards.Select(board => new OrderBoard
            {
                BoardId = board.BoardId,
                OrderBoardComponents = board.Components.Select(component => new OrderBoardComponent
                {
                    ComponentId = component.ComponentId,
                    Quantity = component.Quantity
                }).ToList()
            }).ToList()
        };

        var createdOrder = await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order creation completed for order {OrderId}.", createdOrder.Id);
        return createdOrder;
    }

    public async Task<Order?> UpdateAsync(Guid id, string name, string description, DateTime orderDate)
    {
        _logger.LogInformation("Updating order {OrderId}.", id);

        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder is null)
        {
            _logger.LogWarning("Update requested for missing order {OrderId}.", id);
            return null;
        }

        existingOrder.Name = name.Trim();
        existingOrder.Description = description.Trim();
        existingOrder.OrderDate = orderDate;

        var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
        _logger.LogInformation("Order {OrderId} was successfully updated.", updatedOrder.Id);
        return updatedOrder;
    }
    public async Task<bool?> CanDeleteAsync(Guid id)
    {
        _logger.LogInformation("CanDelete request received for order {OrderId}.", id);
        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return null;
        }
        if(order.Status != OrderStatus.Pending) 
        {
            _logger.LogWarning("Delete operation failed because order {OrderId} has status {Status}.", id, order.Status);
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Delete request received for order {OrderId}.", id);
        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return false;
        }
        
        var deleted = await _orderRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete operation failed because order {OrderId} was not found.", id);
        }

        return deleted;
    }

}
