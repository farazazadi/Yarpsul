using System.ComponentModel.DataAnnotations;

namespace Yarpsul.Shared.ServiceRegistry;

public sealed class ServiceRegistryConfiguration
{
    public static string SectionName => "ServiceRegistry";
    public bool Enabled { get; init; } = true;


    private readonly string _serviceName = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public required string ServiceName
    {
        get => _serviceName;
        init => _serviceName = value.Trim();
    }

    public string HealthEndpoint { get; set; } = string.Empty;


    private readonly List<string>? _tags;
    public List<string>? Tags
    {
        get => _tags;
        init => _tags = value?.ConvertAll(t=> t.Trim());
    }
}