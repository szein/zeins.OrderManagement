#nullable disable
using core.Models;
using FakeItEasy.Sdk;
public class OrderFactory
{
    private string _name;
    private List<OrderBoard> _orderBoards = new List<OrderBoard>();

    public OrderFactory WithName(string name)
    {
        _name = name;
        return this;
    }
    public OrderFactory WithBoardAndComponent(int boardId, int componentId, int quantity)
    {
        _orderBoards.Add(new OrderBoard
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            OrderBoardComponents = new List<OrderBoardComponent>
                {
                    new OrderBoardComponent { ComponentId = componentId, Quantity = quantity }
                }
        });
        return this;
    }

    public Order Create()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            Name = _name,
            Description = $"{_name} Description",
            OrderDate = DateTime.Now,
            OrderBoards = _orderBoards.ToList()
        };
    }
}