using System.Text.Json.Serialization;

namespace core.Models;

public class Board : Auditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Length { get; set; }
    public double Width { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
    
}

