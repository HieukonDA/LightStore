using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

public class IpRateLimitAttribute : ActionFilterAttribute
{
    private readonly int _maxRequests;
    private readonly int _windowMinutes;

    public IpRateLimitAttribute(int maxRequests, int windowMinutes)
    {
        _maxRequests = maxRequests;
        _windowMinutes = windowMinutes;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var rateLimitService = context.HttpContext.RequestServices.GetRequiredService<IpRateLimitService>();
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        var endpoint = $"{context.ActionDescriptor.RouteValues["controller"]}/{context.ActionDescriptor.RouteValues["action"]}";

        var allowed = await rateLimitService.IsAllowedAsync(ipAddress, endpoint);

        if (!allowed)
        {
            context.Result = new StatusCodeResult(429);
            return;
        }

        await next();
    }
}
