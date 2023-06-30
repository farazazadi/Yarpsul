namespace Yarpsul.Shared.InstanceId;

public sealed class InstanceIdProvider
{
    public string InstanceId { get; }

    public InstanceIdProvider()
    {
        InstanceId = Guid.NewGuid().GetHashCode().ToString("X");
    }
}