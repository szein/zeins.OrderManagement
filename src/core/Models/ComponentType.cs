using System.Text.Json.Serialization;

namespace core.Models;

public class ComponentType : Auditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public Component Component { get; set; } = null!;
}
