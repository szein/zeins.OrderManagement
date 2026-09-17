public record ComponentResponse(int Id, int Quantity, ComponentTypeResponse ComponentType);
public record ComponentTypeResponse(int Id, string Name);