using Microsoft.Extensions.DependencyInjection;
using TheLightStore.Application.Interfaces.Infrastructures;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Services;
using TheLightStore.Application.Services.SysServices;

namespace TheLightStore.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        
        // Rate Limit Service
        services.AddScoped<IIpRateLimitService, IpRateLimitService>();

        // Product Services
        services.AddScoped<IPowerService, PowerService>();
        services.AddScoped<IShapeService, ShapeService>();
        services.AddScoped<IColorTemperatureService, ColorTemperatureService>();

        return services;
    }
}
