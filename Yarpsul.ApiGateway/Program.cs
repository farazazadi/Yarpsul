using Yarpsul.ApiGateway.Yarp;
using Yarpsul.Shared.InstanceId;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddInstanceIdProvider();
    
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .DiscoverFromConsul();

var app = builder.Build();


app.MapInstanceIdEndpoint("/", "API Gateway");

app.MapReverseProxy();

app.Run();