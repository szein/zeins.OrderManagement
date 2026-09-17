public interface IUserContext
{
    string? UserId { get; set; }
    string? Email { get; set; }
    string? TenantId { get; set; }
}

public class UserContext : IUserContext
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? TenantId { get; set; }
}