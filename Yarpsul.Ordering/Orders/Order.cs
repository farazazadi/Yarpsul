namespace Yarpsul.Ordering.Orders;

internal record Order(int Id, string CustomerFullName, List<OrderItem> OrderItems, decimal TotalPrice);