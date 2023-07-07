using Yarpsul.Catalog.Products;
using Yarpsul.Shared;
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

app.UseInstanceIdResponseHeader();

app.MapInstanceIdEndpoint("/", "Catalog service");


app.MapGet("/api/products", (ProductService service, InstanceIdProvider instanceIdProvider) =>
{
    var result = WrappedResult<List<Product>>
        .Create(service.GetAllProducts(), instanceIdProvider.InstanceId);

    return Results.Ok(result);
});


app.MapGet("/api/products/{id:int}", (int id, ProductService service, InstanceIdProvider instanceIdProvider) =>
{
    Product? product = service.GetProduct(id);

    var result = WrappedResult<Product?>.Create(product, instanceIdProvider.InstanceId);

    return product is null ? Results.NotFound() : Results.Ok(result);
});


app.Run();