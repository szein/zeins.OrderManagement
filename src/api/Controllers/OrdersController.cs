using Microsoft.AspNetCore.Mvc;
using core.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        _logger.LogInformation("GET /api/orders requested.");
        try
        {
            var orders = await _orderService.GetAllAsync();
            _logger.LogInformation("Returning {OrderCount} orders.", orders?.Count);
            return Ok(orders);
        }
        catch (Exception ex) { return HandleException(ex, "GetAll"); }
    }

    [HttpGet("{id:guid}/boards")]
    public async Task<ActionResult<List<OrderBoardRequest>>> GetBoards(Guid id)
    {
        try
        {
            var boards = await _orderService.GetBoardsAsync(id);
            return Ok(boards);
        }
        catch (Exception ex) { return HandleException(ex, $"GetBoards({id})"); }
    }

    [HttpGet("{id:guid}/boards/{orderBoardId:guid}/components")]
    public async Task<ActionResult<List<BoardComponentRequest>>> GetComponents(Guid id, Guid orderBoardId)
    {
        try
        {
            var components = await _orderService.GetComponentsAsync(id, orderBoardId);
            return Ok(components.Select(orderBoardComponent => new BoardComponentRequest(
                orderBoardComponent.OrderBoardId,
                orderBoardComponent.ComponentId,
                orderBoardComponent.Component?.ComponentType?.Name ?? string.Empty,
                orderBoardComponent.Quantity,
                orderBoardComponent.Component?.Status ?? ComponentStatus.OutOfStock)));
        }
        catch (Exception ex) { return HandleException(ex, $"GetComponents({id}, {orderBoardId})"); }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/orders/{OrderId} requested.", id);
        try
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Order {OrderId} returned successfully.", id);
            return Ok(order);
        }
        catch (Exception ex) { return HandleException(ex, $"GetById({id})"); }
    }

    [HttpGet("{id:guid}/export")]
    public async Task<ActionResult<OrderExportResponse>> Export(Guid id)
    {
        _logger.LogInformation("GET /api/orders/{OrderId}/export requested.", id);
        try
        {
            var order = await _orderService.GetExportDataAsync(id);
            if (order is null)
            {
                _logger.LogWarning("Order export requested for missing order {OrderId}.", id);
                return NotFound();
            }

            _logger.LogInformation("Order export for {OrderId} returned successfully.", id);
            return Ok(order);
        }
        catch (Exception ex) { return HandleException(ex, $"Export({id})"); }
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("POST /api/orders requested for order {OrderName}.", request?.Name ?? "unknown");

        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Create order failed because the request was invalid.");
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _orderService.CreateAsync(request);
            _logger.LogInformation("Order {OrderId} created successfully.", order.Id);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Create");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Order>> Update(Guid id, [FromBody] UpdateOrderRequest request)
    {
        _logger.LogInformation("PUT /api/orders/{OrderId} requested.", id);

        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Update order failed because the request was invalid for order {OrderId}.", id);
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _orderService.UpdateAsync(id, request.Name, request.Description, request.OrderDate);
            if (order is null)
            {
                _logger.LogWarning("Update failed because order {OrderId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Order {OrderId} updated successfully.", id);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order {OrderId} could not be updated because of its current status.", id);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Update({id})");
        }
    }
   

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/orders/{OrderId} requested.", id);
        bool deleted;
        try
        {
            var canDeleted = await _orderService.CanDeleteAsync(id);
            
            if (canDeleted == null)
            {
                _logger.LogInformation("Counld not found an Order with id: {id}", id);
                return NotFound("No order with this id was found!");
            }
            if(!canDeleted.Value)
            {
                _logger.LogInformation("Order {OrderId} could not be deleted because of its current status.", id);
                return Conflict("Order could not be deleted because of its current status.");
            }
            deleted = await _orderService.DeleteAsync(id);
            if(!deleted)
            {
                _logger.LogWarning("Order could not be deleted!");
                return Conflict("Order could not be deleted");
            }
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Delete({id})");
        }

        _logger.LogInformation("Order {OrderId} deleted successfully.", id);
        return NoContent();
    }

    private ObjectResult HandleException(Exception exception, string operation)
    {
        _logger.LogError(exception, "Unhandled exception occurred in {Operation}.", operation);
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = exception.Message
        });
    }
}

