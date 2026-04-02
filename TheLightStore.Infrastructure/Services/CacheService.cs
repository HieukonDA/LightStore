using System;
using Microsoft.Extensions.Caching.Memory;
using TheLightStore.Application.Interfaces.Infrastructures;

namespace TheLightStore.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Set(string key, string subKey, string value, int expiryMinutes)
    {
        var cacheKey = $"{key}:{subKey}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
        };
        _memoryCache.Set(cacheKey, value, cacheOptions);
    }

    public string? Get(string key, string subKey)
    {
        var cacheKey = $"{key}:{subKey}";
        return _memoryCache.TryGetValue(cacheKey, out var value) ? value?.ToString() : null;
    }

    public void Remove(string key, string subKey)
    {
        var cacheKey = $"{key}:{subKey}";
        _memoryCache.Remove(cacheKey);
    }
}
