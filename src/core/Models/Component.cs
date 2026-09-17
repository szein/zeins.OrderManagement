using System.Text.Json.Serialization;

namespace core.Models;

public class Component : Auditable
{
    private ComponentStatus status = ComponentStatus.Available;

    public int Id { get; set; }
    public int ComponentTypeId { get; set; }
    public int Quantity { get; set; } //TODO: convert to uint since quantity cannot be negative.

    public ComponentStatus Status
    {
        get => status == ComponentStatus.Abandoned
            ? ComponentStatus.Abandoned
            : Quantity > 0 ? ComponentStatus.Available : ComponentStatus.OutOfStock;
        set => status = value;
    }
    
    public ComponentType ComponentType { get; set; } = null!;
    
}
