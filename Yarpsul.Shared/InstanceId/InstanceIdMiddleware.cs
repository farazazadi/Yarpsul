using Microsoft.AspNetCore.Http;

namespace Yarpsul.Shared.InstanceId;

public class InstanceIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly InstanceIdProvider _instanceIdProvider;

    public InstanceIdMiddleware(RequestDelegate next, InstanceIdProvider instanceIdProvider)
    {
        _next = next;
        _instanceIdProvider = instanceIdProvider;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add("X-InstanceId", _instanceIdProvider.InstanceId);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}