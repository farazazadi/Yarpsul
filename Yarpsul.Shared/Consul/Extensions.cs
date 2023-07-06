using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Yarpsul.Shared.Consul;

public static class Extensions
{
    public static IServiceCollection AddConsul(this IServiceCollection services)
    {
        services
            .AddOptions<ConsulClientConfiguration>()
            .BindConfiguration("Consul");

        services.AddSingleton<IConsulClient, ConsulClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ConsulClientConfiguration>>();

            return new ConsulClient(options.Value);
        });

        return services;
    }

}