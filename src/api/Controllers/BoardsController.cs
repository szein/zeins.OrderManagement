#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(IBoardService boardService, ILogger<BoardsController> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Board>>> GetAll()
    {
        _logger.LogInformation("GET /api/boards requested.");
        try
        {
            var boards = await _boardService.GetAllAsync();
            _logger.LogInformation("Returning {BoardCount} boards.", boards?.Count);
            return Ok(boards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in GetAll");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = ex.Message
            });
        }
    }

    [HttpGet("{id:int}/components")]
    public async Task<ActionResult<List<BoardComponentRequest>>> GetComponents(int id)
    {
        try
        {
            var components = await _boardService.GetComponentsAsync(id);
            return Ok(components.Select(boardComponent => new BoardComponentRequest(
                boardComponent.OrderBoardId,
                boardComponent.ComponentId,
                boardComponent.Component?.ComponentType?.Name ?? string.Empty,
                boardComponent.Quantity,
                boardComponent.Component?.Status ?? ComponentStatus.OutOfStock)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in GetComponent({id})", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred. While fetching data.",
                Detail = ex.Message
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Board>> GetById(int id)
    {
        _logger.LogInformation("GET /api/boards/{BoardId} requested.", id);
        try
        {

            var board = await _boardService.GetByIdAsync(id);
            if (board is null)
            {
                _logger.LogWarning("Board {BoardId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Board {BoardId} returned successfully.", id);
            return Ok(board);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in GetById({id})", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred. While fetching data.",
                Detail = ex.Message
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Board>> Create([FromBody] CreateBoardRequest request)
    {
        _logger.LogInformation("POST /api/boards requested for board {BoardName}.", request?.Name ?? "unknown");

        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Create board failed because the request body was null.");
            return BadRequest(ModelState);
        }

        try
        {
            var board = await _boardService.CreateAsync(request.Name, request.Description, request.Length, request.Width);
            _logger.LogInformation("Board {BoardId} created successfully.", board.Id);
            return CreatedAtAction(nameof(GetById), new { id = board.Id }, board);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in Create)");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred while saving Board data!",
                Detail = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Board>> Update(int id, [FromBody] UpdateBoardRequest request)
    {
        _logger.LogInformation("PUT /api/boards/{BoardId} requested.", id);

        if (request is null || !ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var board = await _boardService.UpdateAsync(id, request.Name, request.Description, request.Length, request.Width);
            if (board is null)
            {
                _logger.LogWarning("Update failed because board {BoardId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Board {BoardId} updated successfully.", id);
            return Ok(board);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in Update)");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred while saving Board data!",
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/boards/{BoardId} requested.", id);
        try
        {
            var deleted = await _boardService.DeleteAsync(id);

            if (!deleted is null)
            {
                _logger.LogWarning("Delete failed because board {BoardId} was not found.", id);
                return NotFound();
            }
            if (!deleted.Value)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, new ProblemDetails
                {
                    Status = StatusCodes.Status406NotAcceptable,
                    Title = "Board cannot be deleted because it is referenced by an order"
                });
            }

            _logger.LogInformation("Board {BoardId} deleted successfully.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred in Delete");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred while deleting data.",
                Detail = ex.Message
            });
        }
    }
}
