using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TheLightStore.Application.Services;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Application Services
        services.AddScoped<OrderProcessingService>();
        services.AddScoped<IIpRateLimitService, IpRateLimitService>();
        
        // AutoMapper - sẽ cấu hình sau
        // services.AddAutoMapper(assembly);
        
        // FluentValidation - sẽ cấu hình sau
        // services.AddValidatorsFromAssembly(assembly);
        
        // MediatR - sẽ cấu hình sau
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        return services;
    }
}
