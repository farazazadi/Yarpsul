using Yarpsul.Ordering.Orders;
using Yarpsul.Shared.InstanceId;
using Yarpsul.Shared.ServiceRegistry.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddInstanceIdProvider()
    .AddConsulServiceRegistrationBackgroundService()
    .AddSingleton<OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .UseHttpsRedirection()
    .UseInstanceIdResponseHeader();

app.MapInstanceIdEndpoint("/","Ordering service");

app.MapGet("/api/orders", (OrderService service) => Results.Ok(service.GetAllOrders()));

app.MapGet("/api/orders/{id:int}", (OrderService service, int id) =>
{
    Order? order = service.GetOrder(id);

    return order is null ? Results.NotFound() : Results.Ok(order);
});


app.Run();