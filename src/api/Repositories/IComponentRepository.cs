public interface IComponentRepository
{
    Task<List<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<Component> AddAsync(Component component);
    Task<Component> UpdateAsync(Component component);
    Task<bool?> DeleteAsync(int id);
    Task<int> GetAvailableQuantityAsync(int componentTypeId);
}
