using Microsoft.Extensions.Caching.Memory;
using System.Net;

public class IpRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IpRateLimitService> _logger;

    public IpRateLimitService(IMemoryCache cache, ILogger<IpRateLimitService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<bool> IsAllowedAsync(IPAddress ipAddress, string endpoint = "")
    {
        var key = $"rate_limit:{ipAddress}:{endpoint}";
        var config = GetRateLimitConfig(endpoint);

        var requestInfo = _cache.Get<RequestInfo>(key) ?? new RequestInfo();

        if (DateTime.UtcNow - requestInfo.WindowStart > config.Window)
        {
            requestInfo.WindowStart = DateTime.UtcNow;
            requestInfo.RequestCount = 0;
        }

        if (requestInfo.RequestCount >= config.MaxRequests)
        {
            _logger.LogWarning($"Rate limit exceeded for IP {ipAddress} on endpoint {endpoint}");
            return Task.FromResult(false);
        }

        requestInfo.RequestCount++;
        requestInfo.LastRequest = DateTime.UtcNow;

        _cache.Set(key, requestInfo, config.Window);
        return Task.FromResult(true);
    }

    public Task<RateLimitStatus> GetStatusAsync(IPAddress ipAddress, string endpoint = "")
    {
        var key = $"rate_limit:{ipAddress}:{endpoint}";
        var config = GetRateLimitConfig(endpoint);
        var requestInfo = _cache.Get<RequestInfo>(key) ?? new RequestInfo();

        var remainingRequests = Math.Max(0, config.MaxRequests - requestInfo.RequestCount);
        var resetTime = requestInfo.WindowStart.Add(config.Window);

        var status = new RateLimitStatus
        {
            IsAllowed = remainingRequests > 0,
            RemainingRequests = remainingRequests,
            ResetTime = resetTime,
            RetryAfterSeconds = resetTime > DateTime.UtcNow
                ? (int)(resetTime - DateTime.UtcNow).TotalSeconds
                : 0
        };

        return Task.FromResult(status);
    }

    private RateLimitConfig GetRateLimitConfig(string endpoint)
    {
        return endpoint.ToLower() switch
        {
            "/api/auth/login" => new RateLimitConfig { MaxRequests = 5, Window = TimeSpan.FromMinutes(15) },
            "/api/auth/register" => new RateLimitConfig { MaxRequests = 3, Window = TimeSpan.FromHours(1) },
            "/api/auth/forgot-password" => new RateLimitConfig { MaxRequests = 2, Window = TimeSpan.FromMinutes(30) },
            _ => new RateLimitConfig { MaxRequests = 100, Window = TimeSpan.FromMinutes(1) }
        };
    }
}
