using System;
using Microsoft.Extensions.Configuration;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public  string? GetJwtKey()
    {
        return _configuration["Jwt:Key"];
    }

    public string? GetJwtRefreshKey()
    {
        return _configuration["Jwt:RefreshKey"];
    }

    public string? GetJwtIssuer()
    {
        return _configuration["Jwt:Issuer"];
    }

    public string? GetJwtAudience()
    {
        return _configuration["Jwt:Audience"];
    }

    public string? GetConfirmEmailUrl()
    {
        return _configuration["Auth:ConfirmEmailUrl"];
    }

    public int GetOtpExpiryMinutes()
    {
        return int.Parse(_configuration["Auth:OtpExpiryMinutes"] ?? "5");
    }
}
