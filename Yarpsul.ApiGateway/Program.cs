using Yarpsul.Shared.InstanceId;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddInstanceIdProvider();

var app = builder.Build();

app
    .UseHttpsRedirection()
    .UseInstanceIdResponseHeader();

app.MapInstanceIdEndpoint("/", "API Gateway");

app.Run();
