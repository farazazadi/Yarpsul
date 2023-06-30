using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Yarpsul.Shared.InstanceId;

public static class Extensions
{
    public static IServiceCollection AddInstanceIdProvider(this IServiceCollection services)
    {
        services.AddSingleton<InstanceIdProvider>();
        return services;
    }


    /// <summary>
    /// Adds middleware to enrich response header with InstanceId.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance this method extends.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for UseInstanceIdProvider.</returns>
    public static IApplicationBuilder UseInstanceIdResponseHeader(this IApplicationBuilder app)
    {

        app.UseMiddleware<InstanceIdMiddleware>();

        return app;
    }


    public static RouteHandlerBuilder MapInstanceIdEndpoint(
        this IEndpointRouteBuilder endpoints,
        string routingPath,
        string serviceName)
    {

        RouteHandlerBuilder builder = endpoints.MapGet(routingPath,
            (InstanceIdProvider instanceIdProvider) => $"{serviceName}, InstanceId: {instanceIdProvider.InstanceId}");

        return builder;
    }


}