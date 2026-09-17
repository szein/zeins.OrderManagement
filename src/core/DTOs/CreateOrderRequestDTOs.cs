
using System.ComponentModel.DataAnnotations;

public record CreateOrderRequest(
	[Required(ErrorMessage = "Order name is required.")] string Name,
	string Description,
	[Range(typeof(DateTime), "1900-01-01", "9999-12-31", ErrorMessage = "Order date is required.")] DateTime OrderDate,
	[Required(ErrorMessage = "At least one board is required.")] List<CreateOrderBoardRequest> Boards);
public record CreateOrderBoardRequest(
	[Range(1, int.MaxValue, ErrorMessage = "Board ID must be greater than zero.")] int BoardId,
	[Required(ErrorMessage = "At least one component is required.")] List<CreateOrderBoardComponentRequest> Components);
public record CreateOrderBoardComponentRequest(
	[Range(1, int.MaxValue, ErrorMessage = "Component ID must be greater than zero.")] int ComponentId,
	[Range(1, int.MaxValue, ErrorMessage = "Component quantity must be greater than zero.")] int Quantity);

public record OrderBoardRequest(Guid OrderBoardId, int BoardId, string Name, string Description, double Length, double Width);