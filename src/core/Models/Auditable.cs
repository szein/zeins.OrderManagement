
namespace core.Models;

public abstract class Auditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? LastModifiedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
}

