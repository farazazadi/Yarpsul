using Yarpsul.Shared.InstanceId;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddInstanceIdProvider();

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

app.Run();

