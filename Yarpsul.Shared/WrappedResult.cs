namespace Yarpsul.Shared;

public sealed class WrappedResult<T>
{
    public string DownStreamServiceInstanceId { get; init; }
    public T Value { get; init; }

    private WrappedResult(T value, string downStreamServiceInstanceId)
    {
        Value = value;
        DownStreamServiceInstanceId = downStreamServiceInstanceId;
    }

    public static WrappedResult<T> Create(T value, string instanceId) => new(value, instanceId);
}