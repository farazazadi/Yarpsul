using Consul;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;

namespace Yarpsul.ApiGateway.Yarp;

internal sealed class ProxyConfigProvider : IProxyConfigProvider, IDisposable
{
    private readonly InMemoryConfigProvider _provider = new(Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());

    private readonly IProxyConfigProvider[] _otherProviders;
    private readonly IConsulClient _consulClient;
    private readonly IOptionsMonitor<ServiceDiscoveryConfiguration> _serviceDiscoveryOptions;

    private readonly CancellationTokenSource _stoppingToken = new();

    public ProxyConfigProvider(IEnumerable<IProxyConfigProvider> providers, IConsulClient consulClient,
        IOptionsMonitor<ServiceDiscoveryConfiguration> serviceDiscoveryOptions)
    {
        _otherProviders = providers.ToArray();
        _consulClient = consulClient;
        _serviceDiscoveryOptions = serviceDiscoveryOptions;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        PeriodicUpdateAsync(_stoppingToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

    }


    public IProxyConfig GetConfig() => _provider.GetConfig();


    private async Task PeriodicUpdateAsync(CancellationToken stoppingToken)
    {
        try
        {
            do
            {
                var delay = TimeSpan.FromSeconds(_serviceDiscoveryOptions.CurrentValue.PeriodicUpdateIntervalInSeconds);

                await UpdateAsync(stoppingToken);
                await Task.Delay(delay, stoppingToken);

            } while (!stoppingToken.IsCancellationRequested);

        }
        catch (TaskCanceledException) { }
    }

    private async Task UpdateAsync(CancellationToken stoppingToken)
    {
        List<ClusterConfig> clusters = new();
        List<RouteConfig> routes = new();

        foreach (var provider in _otherProviders)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            var defaultConfig = provider.GetConfig();

            routes.AddRange(defaultConfig.Routes);

            var clusterConfigs = await CreateClusters(defaultConfig, stoppingToken);
            clusters.AddRange(clusterConfigs);
        }

        _provider.Update(routes, clusters);
    }

    private async Task<List<ClusterConfig>> CreateClusters(IProxyConfig defaultConfig, CancellationToken stoppingToken)
    {
        var getServicesFromConsulResult = await _consulClient.Agent.Services(stoppingToken);
        var discoveredServices = getServicesFromConsulResult.Response;

        List<ClusterConfig> clusters = new();

        foreach (var cluster in defaultConfig.Clusters)
        {
            Dictionary<string, DestinationConfig> destinations = CreateDestinationsForCluster(cluster, discoveredServices);

            var newCluster = cluster with { Destinations = destinations };

            clusters.Add(newCluster);
        }

        return clusters;
    }


    private Dictionary<string, DestinationConfig> CreateDestinationsForCluster(ClusterConfig defaultCluster,
        Dictionary<string, AgentService> discoveredServices)
    {
        Dictionary<string, DestinationConfig> destinations = new();

        if (defaultCluster.Destinations is null)
            return destinations;

        foreach (KeyValuePair<string, DestinationConfig> defaultDestination in defaultCluster.Destinations)
        {
            string defaultDestinationKey = defaultDestination.Key;
            DestinationConfig defaultDestinationValue = defaultDestination.Value;

            string? placeholder = ExtractDestinationServiceNamePlaceholder(defaultDestinationValue.Address);

            if (placeholder is null)
            {
                destinations.Add(defaultDestinationKey, defaultDestinationValue);
                continue;
            }

            List<AgentService> agentServices = discoveredServices.Values
                .Where(s => s.Service.ToLowerInvariant() == placeholder.ToLowerInvariant())
                .ToList();

            if(!agentServices.Any())
                continue;

            foreach (AgentService agentService in agentServices)
            {
                var destinationName = agentService.ID;

                var healsEndpointExist = agentService.Meta.TryGetValue("HealthEndpoint", out var healthEndpoint);
                
                var schemeExist = agentService.Meta.TryGetValue("Scheme", out var scheme);

                if(!schemeExist)
                    continue;

                var address = $"{scheme}://{agentService.Address}:{agentService.Port}";

                DestinationConfig destinationConfig = new()
                {
                    Address = defaultDestinationValue.Address.Replace($"[{placeholder}]", address),
                    Health = healthEndpoint,
                    Metadata = defaultDestinationValue.Metadata
                };

                destinations.Add(destinationName, destinationConfig);
            }
        }

        return destinations;
    }


    private string? ExtractDestinationServiceNamePlaceholder(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var startIndex = url.IndexOf('[', StringComparison.Ordinal) + 1;

        if (startIndex == 0)
            return null;


        var endIndex = url.IndexOf(']', startIndex);

        if (endIndex == -1)
            return null;


        return url.Substring(startIndex, length: endIndex - startIndex);

    }

    public void Dispose()
    {
        _stoppingToken.Cancel();
        _stoppingToken.Dispose();
    }
}