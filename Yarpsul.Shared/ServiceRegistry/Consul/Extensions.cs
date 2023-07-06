using Consul;
using Microsoft.Extensions.DependencyInjection;
using Yarpsul.Shared.Consul;

namespace Yarpsul.Shared.ServiceRegistry.Consul;

public static class Extensions
{
    public static IServiceCollection AddConsulServiceRegistrationBackgroundService(this IServiceCollection services)
    {
        var consulIsAlreadyRegistered = services.Any(service => service.ServiceType == typeof(IConsulClient));

        if (!consulIsAlreadyRegistered)
            services.AddConsul();

        services
            .AddOptions<ServiceRegistryConfiguration>()
            .BindConfiguration(ServiceRegistryConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<ConsulServiceRegistrationBackgroundService>();

        return services;
    }


}