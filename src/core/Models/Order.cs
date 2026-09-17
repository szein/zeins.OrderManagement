namespace core.Models;

using System.Text.Json.Serialization;

public class Order : Auditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
}

public class OrderBoard
{
    public Guid Id {get; set;}

    public Guid OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; } = null!;

    public int BoardId { get; set; }
    [JsonIgnore]
    public Board Board { get; set; } = null!;

    public ICollection<OrderBoardComponent> OrderBoardComponents { get; set; } = new List<OrderBoardComponent>();
}

public class OrderBoardComponent
{
    public int Id { get; set; }

    public Guid OrderBoardId { get; set; }
    [JsonIgnore]
    public OrderBoard OrderBoard { get; set; } = null!;

    public int ComponentId { get; set; }
    [JsonIgnore]
    public Component Component { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}