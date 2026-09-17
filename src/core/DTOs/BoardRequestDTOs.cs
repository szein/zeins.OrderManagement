
using System.ComponentModel.DataAnnotations;

public record CreateBoardRequest(
    [Required(ErrorMessage ="Board name is required.")]
    string Name, 
    string Description, 
    [Range(0, 999_999_999, ErrorMessage = "Board length must be greater than zero.")]
    double Length, 
    [Range(0, 999_999_999, ErrorMessage = "Board Width must be greater than zero.")]
    double Width);
public record UpdateBoardRequest(
    [Required(ErrorMessage ="Board name is required.")]
    string Name, 
    string Description, 
    [Range(0, 999_999_999, ErrorMessage = "Board length must be greater than zero.")]
    double Length, 
    [Range(0, 999_999_999, ErrorMessage = "Board Width must be greater than zero.")]
    double Width
);
public record BoardComponentRequest(Guid OrderBoardId, int ComponentId, string ComponentTypeName, int Quantity, ComponentStatus Status);
