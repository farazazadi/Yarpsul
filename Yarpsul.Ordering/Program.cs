using Yarpsul.Ordering.Orders;
using Yarpsul.Shared;
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


app.MapGet("/api/orders", (OrderService service, InstanceIdProvider instanceIdProvider) =>
{
    var result = WrappedResult<List<Order>>
        .Create(service.GetAllOrders(), instanceIdProvider.InstanceId);

    return Results.Ok(result);
});


app.MapGet("/api/orders/{id:int}", (OrderService service, InstanceIdProvider instanceIdProvider, int id) =>
{
    Order? order = service.GetOrder(id);

    var result = WrappedResult<Order?>.Create(order, instanceIdProvider.InstanceId);

    return order is null ? Results.NotFound() : Results.Ok(result);
});


app.Run();