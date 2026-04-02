using System;

namespace TheLightStore.WebAPI.Filter;

public class DeviceDetectionService
{
    public bool IsMobile(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();
        return userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad") || userAgent.Contains("mobile");
    }

    public bool IsBrowser(HttpContext context)
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString().ToLower();
        return userAgent.Contains("chrome") || userAgent.Contains("safari") || userAgent.Contains("firefox") || userAgent.Contains("edge") || userAgent.Contains("opera");
    }
}
