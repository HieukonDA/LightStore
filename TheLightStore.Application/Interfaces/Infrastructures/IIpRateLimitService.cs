using System.Net;
using TheLightStore.Application.Models.RateLimit;

namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface IIpRateLimitService
{
    Task<bool> IsAllowedAsync(IPAddress ipAddress, string endpoint = "");
    Task<RateLimitStatus> GetStatusAsync(IPAddress ipAddress, string endpoint = "");
}
