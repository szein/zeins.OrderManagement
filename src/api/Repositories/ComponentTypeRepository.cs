using Microsoft.EntityFrameworkCore;

public class ComponentTypeRepository : IComponentTypeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ComponentTypeRepository> _logger;

    public ComponentTypeRepository(AppDbContext context, ILogger<ComponentTypeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ComponentType>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all component types from the database.");

        return await _context.ComponentTypes
            .Include(ct => ct.Component)
            .AsNoTracking()
            .OrderBy(ct => ct.Id)
            .ToListAsync();
    }

    public async Task<ComponentType?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching component type with Id {ComponentTypeId}.", id);

        return await _context.ComponentTypes
            .Include(ct => ct.Component)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.Id == id);
    }

    public async Task<ComponentType> AddAsync(ComponentType componentType)
    {
        _logger.LogInformation("Adding new component type named {ComponentTypeName}.", componentType.Name);

        await _context.ComponentTypes.AddAsync(componentType);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Component type {ComponentTypeId} was created successfully.", componentType.Id);
        return componentType;
    }

    public async Task<ComponentType> UpdateAsync(ComponentType componentType)
    {
        _logger.LogInformation("Updating component type {ComponentTypeId}.", componentType.Id);

        var trackedComponentType = await _context.ComponentTypes.FindAsync(componentType.Id);
        if (trackedComponentType is null)
        {
            throw new InvalidOperationException($"Component type {componentType.Id} was not found.");
        }

        trackedComponentType.Name = componentType.Name;
        trackedComponentType.Description = componentType.Description;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Component type {ComponentTypeId} was updated successfully.", componentType.Id);
        return trackedComponentType;
    }

    public async Task<bool?> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete component type {ComponentTypeId}.", id);

        var componentType = await _context.ComponentTypes.FirstOrDefaultAsync(ct => ct.Id == id);
        if (componentType is null)
        {
            _logger.LogWarning("Delete requested for component type {ComponentTypeId}, but it was not found.", id);
            return null;
        }

        _context.ComponentTypes.Remove(componentType);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Component type {ComponentTypeId} was deleted successfully.", id);
        return true;
    }
}
