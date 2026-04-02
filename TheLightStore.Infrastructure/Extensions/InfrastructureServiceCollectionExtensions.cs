using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLightStore.Application.Interfaces.Infrastructures;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Infrastructure.Persistence;
using TheLightStore.Infrastructure.Persistence.SysEntities;
using TheLightStore.Infrastructure.Repositories;
using TheLightStore.Infrastructure.Repositories.Auth;
using TheLightStore.Infrastructure.Services;
using TheLightStore.Infrastructure.Services.Identity;

namespace TheLightStore.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        
        services.AddDbContext<DBContext>(options =>
            options.UseSqlServer(connectionString));

        // Identity
        services.AddIdentityCore<Users>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<DBContext>();

        // Infrastructure Services
        services.AddScoped<ICodeService, CodeService>();
        services.AddScoped<IAuthDataService, AuthDataService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Product Repositories
        services.AddScoped<IPowerRepository, PowerRepository>();

        return services;
    }
}
