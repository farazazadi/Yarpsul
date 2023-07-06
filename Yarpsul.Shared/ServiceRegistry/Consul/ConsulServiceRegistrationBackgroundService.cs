using System.Collections.Concurrent;
using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarpsul.Shared.InstanceId;

namespace Yarpsul.Shared.ServiceRegistry.Consul;

public class ConsulServiceRegistrationBackgroundService : BackgroundService
{
    private readonly ILogger<ConsulServiceRegistrationBackgroundService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServer _server;
    private readonly IConsulClient _consulClient;
    private readonly ServiceRegistryConfiguration _serviceRegistryConfiguration;
    private readonly string _serviceId;

    private readonly ConcurrentBag<string> _registeredServicesIds = new();

    public ConsulServiceRegistrationBackgroundService(
        ILogger<ConsulServiceRegistrationBackgroundService> logger,
        IHostApplicationLifetime applicationLifetime,
        IServer server,
        IConsulClient consulClient,
        IOptions<ServiceRegistryConfiguration> serviceRegistryConfiguration,
        InstanceIdProvider instanceIdProvider)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _server = server;
        _consulClient = consulClient;
        _serviceRegistryConfiguration = serviceRegistryConfiguration.Value;

        _serviceId = $"{_serviceRegistryConfiguration.ServiceName} - {instanceIdProvider.InstanceId}";
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRegistryConfiguration.Enabled)
            return;

        if (!await WaitForAppStartup(stoppingToken))
            return;

        await RegisterAppInConsul(stoppingToken);
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceRegistryConfiguration.Enabled)
            await UnRegisterAppInConsul(cancellationToken);

        await base.StopAsync(cancellationToken);
    }


    private async Task RegisterAppInConsul(CancellationToken stoppingToken)
    {
        var serviceName = _serviceRegistryConfiguration.ServiceName;

        List<Uri> appAddresses = GetAppAddresses();

        var appAddressesCount = appAddresses.Count;

        for (var index = 0; index < appAddressesCount; index++)
        {
            Uri appAddress = appAddresses[index];
            List<string> tags = new();

            var registrationId = appAddressesCount > 1 ? $"{_serviceId} - U{index + 1}" : _serviceId;


            if (_serviceRegistryConfiguration.Tags is null || !_serviceRegistryConfiguration.Tags.Any())
            {
                tags.Add(registrationId);
            }
            else
            {
                tags.Add(registrationId);
                tags.AddRange(_serviceRegistryConfiguration.Tags);
            }


            AgentServiceRegistration serviceRegistration = new()
            {
                ID = registrationId,
                Name = serviceName,
                Address = appAddress.Host,
                Port = appAddress.Port,
                Tags = tags.ToArray(),
             };

            var healthEndpoint = _serviceRegistryConfiguration.HealthEndpoint;

            if (!string.IsNullOrWhiteSpace(healthEndpoint))
            {
                serviceRegistration.Meta = new Dictionary<string, string>
                {
                    { "HealthEndpoint", healthEndpoint }
                };
            }

            try
            {
                await _consulClient.Agent.ServiceRegister(serviceRegistration, stoppingToken);
                _registeredServicesIds.Add(registrationId);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, e.Message);
            }
        }
    }


    private async Task UnRegisterAppInConsul(CancellationToken stoppingToken)
    {
        try
        {
            foreach (string registeredServicesId in _registeredServicesIds)
                await _consulClient.Agent.ServiceDeregister(registeredServicesId, stoppingToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, e.Message);
        }
    }


    private List<Uri> GetAppAddresses()
    {
        List<Uri> uris = new();

        var addressFeature = _server.Features.Get<IServerAddressesFeature>();

        if (addressFeature?.Addresses is null)
            return uris;


        foreach (var address in addressFeature.Addresses)
            uris.Add(new Uri(address));

        return uris;
    }


    private async Task<bool> WaitForAppStartup(CancellationToken stoppingToken)
    {
        // Thanks to Andrew Lock
        // https://andrewlock.net/finding-the-urls-of-an-aspnetcore-app-from-a-hosted-service-in-dotnet-6/

        var startedSource = new TaskCompletionSource();
        var cancelledSource = new TaskCompletionSource();

        await using CancellationTokenRegistration reg1 = _applicationLifetime.ApplicationStarted.Register(() => startedSource.SetResult());
        await using CancellationTokenRegistration reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

        Task completedTask = await Task.WhenAny(
                startedSource.Task,
                cancelledSource.Task)
            .ConfigureAwait(false);

        return completedTask == startedSource.Task;
    }
}