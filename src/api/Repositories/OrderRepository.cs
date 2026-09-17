using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    //TODO: Check if it needs improvment
    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all orders from the database.");

        return await _context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    public async Task<List<OrderBoardRequest>> GetBoardsAsync(Guid orderId)
    {
        return await _context.OrderBoards
            .AsNoTracking()
            .Where(orderBoard => orderBoard.OrderId == orderId)
            .Include(orderBoard => orderBoard.Board)
            .OrderBy(orderBoard => orderBoard.BoardId)
            .ThenBy(orderBoard => orderBoard.Id)
            .Select(orderBoard => new OrderBoardRequest(
                orderBoard.Id,
                orderBoard.BoardId,
                orderBoard.Board.Name,
                orderBoard.Board.Description,
                orderBoard.Board.Length,
                orderBoard.Board.Width))
            .ToListAsync();
    }

    public async Task<List<OrderBoardComponent>> GetComponentsAsync(Guid orderId, Guid orderBoardId)
    {
        return await _context.OrderBoardComponents
            .Where(orderBoardComponent =>
                orderBoardComponent.OrderBoard.OrderId == orderId &&
                orderBoardComponent.OrderBoard.Id == orderBoardId)
            .Include(orderBoardComponent => orderBoardComponent.Component)
            .ThenInclude(component => component.ComponentType)
            .AsNoTracking()
            .OrderBy(orderBoardComponent => orderBoardComponent.ComponentId)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching order with Id {OrderId}.", id);

        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<OrderExportResponse?> GetExportDataAsync(Guid id)
    {
        _logger.LogInformation("Fetching export data for order {OrderId}.", id);

        var order = await _context.Orders
            .Include(o => o.OrderBoards)
                .ThenInclude(orderBoard => orderBoard.Board)
            .Include(o => o.OrderBoards)
                .ThenInclude(orderBoard => orderBoard.OrderBoardComponents)
                    .ThenInclude(orderBoardComponent => orderBoardComponent.Component)
                        .ThenInclude(component => component.ComponentType)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return null;
        }

        return new OrderExportResponse(
            order.Id,
            order.Name,
            order.Description,
            order.OrderDate,
            order.Status,
            order.OrderBoards
                .OrderBy(orderBoard => orderBoard.BoardId)
                .Select(orderBoard => new OrderExportBoardResonse(
                    orderBoard.Board.Id,
                    orderBoard.Board.Name,
                    orderBoard.Board.Description,
                    orderBoard.Board.Length,
                    orderBoard.Board.Width,
                    orderBoard.OrderBoardComponents
                        .OrderBy(component => component.ComponentId)
                        .Select(component => new OrderExportComponentResponse(
                            component.ComponentId,
                            component.Component.ComponentType?.Name ?? string.Empty,
                            component.Quantity,
                            component.Component.Status))
                        .ToList()))
                .ToList());
    }

    public async Task<Order> AddAsync(Order order)
    {
        _logger.LogInformation("Adding new order named {OrderName} for date {OrderDate}.", order.Name, order.OrderDate);

        var componentsInRequest = order.OrderBoards
            .SelectMany(b => b.OrderBoardComponents)
            .GroupBy(c => c.ComponentId)
            .Select(g => new 
            {
                ComponentId = g.Key,
                TotalQuantity = g.Sum(c => c.Quantity)
            })
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync();
        foreach (var componentInRequest in componentsInRequest)
        {
            _logger.LogDebug("Component {componentId} with Total Quantity {Quantity} in request",componentInRequest.ComponentId, componentInRequest.TotalQuantity);
            var componentInDb = _context.Components.Single(c=> c.Id == componentInRequest.ComponentId);
            _logger.LogDebug("Component {componentId} has Quantity {Quantity} in Database",componentInRequest.ComponentId, componentInDb.Quantity);
            
            if(componentInDb.Quantity < componentInRequest.TotalQuantity) throw new ArgumentOutOfRangeException("Quantity in Database is not Sufficent!");
            componentInDb.Quantity -= componentInRequest.TotalQuantity;
        }
        order.Status = OrderStatus.Created;
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        _logger.LogInformation("Order {OrderId} was created successfully.", order.Id);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _logger.LogInformation("Updating order {OrderId}.", order.Id);

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was updated successfully.", order.Id);
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete order {OrderId}.", id);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            _logger.LogWarning("Delete requested for order {OrderId}, but it was not found.", id);
            return false;
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be deleted.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was deleted successfully.", id);
        return true;
    }
}
