namespace Yarpsul.ApiGateway.Yarp;

internal sealed class ServiceDiscoveryConfiguration
{
    public static string SectionName => "ServiceDiscovery";

    public uint PeriodicUpdateIntervalInSeconds { get; init; }
}