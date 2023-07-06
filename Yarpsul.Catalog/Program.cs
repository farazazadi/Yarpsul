using Yarpsul.Catalog.Products;
using Yarpsul.Shared.InstanceId;
using Yarpsul.Shared.ServiceRegistry.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddInstanceIdProvider()
    .AddConsulServiceRegistrationBackgroundService()
    .AddSingleton<ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .UseHttpsRedirection()
    .UseInstanceIdResponseHeader();

app.MapInstanceIdEndpoint("/", "Catalog service");


app.MapGet("/api/products", (ProductService service) => Results.Ok(service.GetAllProducts()));


app.MapGet("/api/products/{id:int}", (ProductService service, int id) =>
{
    Product? product = service.GetProduct(id);

    return product is null ? Results.NotFound() : Results.Ok(product);
});


app.Run();