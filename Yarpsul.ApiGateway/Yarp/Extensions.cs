using Consul;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;
using Yarpsul.Shared.Consul;

namespace Yarpsul.ApiGateway.Yarp;

internal static class Extensions
{
    public static IReverseProxyBuilder DiscoverFromConsul(this IReverseProxyBuilder builder)
    {
        var services = builder.Services;

        AddConsulIfNotExist(services);

        services.AddOptions<ServiceDiscoveryConfiguration>()
            .BindConfiguration(ServiceDiscoveryConfiguration.SectionName)
            .ValidateOnStart();


        var serviceProvider = services.BuildServiceProvider();


        var proxyConfigProviders = serviceProvider.GetRequiredService<IEnumerable<IProxyConfigProvider>>();

        var consulClient = serviceProvider.GetRequiredService<IConsulClient>();

        var options = serviceProvider.GetRequiredService<IOptionsMonitor<ServiceDiscoveryConfiguration>>();

        services.RemoveAll<IProxyConfigProvider>();

        services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>(_=> new ProxyConfigProvider(proxyConfigProviders, consulClient, options));

        return builder;
    }

    private static void AddConsulIfNotExist(IServiceCollection services)
    {
        var consulIsAlreadyRegistered = services.Any(service => service.ServiceType == typeof(IConsulClient));

        if (!consulIsAlreadyRegistered)
            services.AddConsul();
    }
}