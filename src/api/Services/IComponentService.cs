using core.Models;
public interface IComponentService
{
    Task<List<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<Component> CreateAsync(string name, string description, int quantity);
    Task<Component?> UpdateAsync(int id, string name, string description, int quantity);
    Task<bool?> DeleteAsync(int id);
    Task<int> GetAvailableQuantityAsync(int? componentTypeId, ComponentType? componentType);
}
