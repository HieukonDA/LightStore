using Microsoft.Extensions.DependencyInjection;
using TheLightStore.Application.Interfaces;
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

        return services;
    }
}
