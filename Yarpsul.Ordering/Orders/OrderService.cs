namespace Yarpsul.Ordering.Orders;

internal sealed class OrderService
{
    private readonly List<Order> _orders;

    public OrderService()
    {
        List<OrderItem> order1Items = new()
        {
            new OrderItem(1, 1, 35.99m),
            new OrderItem(2, 5, 47.91m),
        };

        List<OrderItem> order2Items = new()
        {
            new OrderItem(3, 2, 46.82m),
            new OrderItem(4, 3, 24.00m),
            new OrderItem(5, 4, 47.99m),
        };

        _orders = new List<Order>
        {
            new Order(1, "Jan Lewis", order1Items, order1Items.Sum(o => o.ProductPrice)),
            new Order(2, "Hannah Bray", order2Items, order2Items.Sum(o => o.ProductPrice))
        };
    }


    public List<Order> GetAllOrders () => _orders;

    public Order? GetOrder (int id) => _orders.FirstOrDefault(o => o.Id == id);

}