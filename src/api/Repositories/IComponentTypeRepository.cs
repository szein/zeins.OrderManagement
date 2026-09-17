public interface IComponentTypeRepository
{
    /// <summary>
    /// Get all ComponentTypes
    /// </summary>
    /// <returns></returns>
    Task<List<ComponentType>> GetAllAsync();
    /// <summary>
    /// Get Component by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ComponentType?> GetByIdAsync(int id);
    /// <summary>
    /// Add ComponentType
    /// </summary>
    /// <param name="componentType"></param>
    /// <returns></returns>
    Task<ComponentType> AddAsync(ComponentType componentType);
    /// <summary>
    /// Update Component
    /// </summary>
    /// <param name="componentType"></param>
    /// <returns></returns>
    Task<ComponentType> UpdateAsync(ComponentType componentType);
}
