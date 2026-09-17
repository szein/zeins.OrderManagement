using core.Models;

public record OrderExportResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime OrderDate,
    OrderStatus Status,
    List<OrderExportBoardResonse> Boards);

public record OrderExportBoardResonse(
    int Id,
    string Name,
    string Description,
    double Length,
    double Width,
    List<OrderExportComponentResponse> Components);

public record OrderExportComponentResponse(
    int Id,
    string ComponentTypeName,
    int Quantity,
    ComponentStatus Status);