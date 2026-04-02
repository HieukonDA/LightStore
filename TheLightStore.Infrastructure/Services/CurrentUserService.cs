using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public string? GetCurrentUserId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User == null)
        {
            _logger.LogWarning("HttpContext or User is null");
            return null;
        }

        // Try multiple claim types
        var userId = context.User.FindFirst("sub")?.Value
                  ?? context.User.FindFirst("UserId")?.Value
                  ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation($"Current UserId from claims: {userId ?? "NOT FOUND"}");
        
        // Debug: Log all claims
        var allClaims = string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}"));
        _logger.LogDebug($"All claims: {allClaims}");

        return userId;
    }

    public string? GetUserEmail()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.User.FindFirst("email")?.Value
            ?? context?.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }

    public string? GetUsername()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.User.FindFirst("username")?.Value
            ?? context?.User.FindFirst("UserName")?.Value
            ?? context?.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }
}
