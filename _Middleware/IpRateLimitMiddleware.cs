using System.Net;

public class IpRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IpRateLimitService _rateLimitService;

    public IpRateLimitMiddleware(RequestDelegate next, IpRateLimitService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = GetClientIpAddress(context);
        var endpoint = context.Request.Path.Value;

        if (IsLocalhost(ipAddress) &&
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            await _next(context);
            return;
        }

        var status = await _rateLimitService.GetStatusAsync(ipAddress, endpoint);

        if (!status.IsAllowed)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("X-RateLimit-Limit", status.RemainingRequests.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", status.ResetTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            context.Response.Headers.Add("Retry-After", status.RetryAfterSeconds.ToString());

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests from IP {ipAddress}",
                retryAfter = status.RetryAfterSeconds
            }));
            return;
        }

        await _rateLimitService.IsAllowedAsync(ipAddress, endpoint);
        var newStatus = await _rateLimitService.GetStatusAsync(ipAddress, endpoint);

        context.Response.Headers.Add("X-RateLimit-Remaining", newStatus.RemainingRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", newStatus.ResetTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));

        await _next(context);
    }

    private IPAddress GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            var ips = xForwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (IPAddress.TryParse(ips[0].Trim(), out var forwardedIp))
                return forwardedIp;
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp) && IPAddress.TryParse(xRealIp, out var realIp))
            return realIp;

        return context.Connection.RemoteIpAddress ?? IPAddress.Loopback;
    }

    private bool IsLocalhost(IPAddress ipAddress)
    {
        return IPAddress.IsLoopback(ipAddress) ||
               ipAddress.Equals(IPAddress.Parse("::1")) ||
               ipAddress.Equals(IPAddress.Parse("127.0.0.1"));
    }
}
