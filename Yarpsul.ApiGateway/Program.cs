using Yarpsul.Shared.InstanceId;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddInstanceIdProvider()
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")); ;

var app = builder.Build();

app.MapReverseProxy();

app.UseHttpsRedirection();

app.MapInstanceIdEndpoint("/", "API Gateway");

app.Run();
