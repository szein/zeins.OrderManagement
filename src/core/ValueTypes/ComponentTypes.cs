using core.Models;
namespace core.ValueTypes;

public static class ComponentTypes
{

    public static ComponentType BGA => new ComponentType
    {
        Id  = 1,
        Name = "BGA",
        Description = "Ball Grid Array Package"
    };
}