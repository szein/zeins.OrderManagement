using Microsoft.AspNetCore.Mvc;
using core.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComponentsController : ControllerBase
{
    private readonly IComponentService _componentService;
    private readonly ILogger<ComponentsController> _logger;

    public ComponentsController(
        IComponentService componentService,
        ILogger<ComponentsController> logger)
    {
        _componentService = componentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Component>>> GetAll()
    {
        _logger.LogInformation("GET /api/components requested.");
        try
        {
            var components = await _componentService.GetAllAsync();
            _logger.LogInformation("Returning {ComponentCount} components.", components?.Count);
            return Ok(components);
        }
        catch (Exception ex) { return HandleException(ex, "GetAll"); }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Component>> GetById(int id)
    {
        _logger.LogInformation("GET /api/components/{ComponentId} requested.", id);
        try
        {
            var component = await _componentService.GetByIdAsync(id);
            if (component is null)
            {
                _logger.LogWarning("Component {ComponentId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Component {ComponentId} returned successfully.", id);
            return Ok(component);
        }
        catch (Exception ex) { return HandleException(ex, $"GetById({id})"); }
    }

    [HttpPost]
    public async Task<ActionResult<Component>> Create([FromBody] CreateComponentRequest request)
    {
        _logger.LogInformation("POST /api/components requested for component type {ComponentTypeName}.", request?.Name);

        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Create component failed because the request was invalid.");
            return BadRequest(ModelState);
        }

        try
        {
            var component = await _componentService.CreateAsync(request.Name, request.Description, request.Quantity);
            _logger.LogInformation("Component {ComponentId} created successfully.", component.Id);
            return CreatedAtAction(nameof(GetById), new { id = component.Id }, component);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Create");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Component>> Update(int id, [FromBody] UpdateComponentRequest request)
    {
        _logger.LogInformation("PUT /api/components/{ComponentId} requested.", id);

        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Update component failed because the request was invalid for component {ComponentId}.", id);
            return BadRequest(ModelState);
        }

        try
        {
            var component = await _componentService.UpdateAsync(id, request.Name, request.Description, request.Quantity);
            if (component is null)
            {
                _logger.LogWarning("Update failed because component {ComponentId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Component {ComponentId} updated successfully.", id);
            return Ok(component);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Update({id})");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/components/{ComponentId} requested.", id);
        try
        {
            var deleted = await _componentService.DeleteAsync(id);

            if (deleted is null)
            {
                _logger.LogWarning("Delete failed because component {ComponentId} was not found.", id);
                return NotFound();
            }

            if(!deleted.Value)
            {
                _logger.LogWarning("Component {id} cannot be deleted because it is referenced by an order", id);
                return StatusCode(StatusCodes.Status406NotAcceptable, new ProblemDetails
                {
                    Status = StatusCodes.Status406NotAcceptable,
                    Title = "Component cannot be deleted because it is referenced by an order."
                });
            }

            _logger.LogInformation("Component {ComponentId} deleted successfully.", id);
            return NoContent();
        }
        catch (Exception ex) { return HandleException(ex, $"Delete({id})"); }
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
