using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Infrastructure.Persistence;
using TheLightStore.Infrastructure.ExternalServices;
using TheLightStore.Infrastructure.Configuration;
using TheLightStore.Infrastructure.Repositories;
using TheLightStore.Infrastructure.Repositories.Orders;
using TheLightStore.Infrastructure.Repositories.Products;
using TheLightStore.Infrastructure.Repositories.ProductReviews;
using TheLightStore.Infrastructure.Repositories.Auth;
using TheLightStore.Infrastructure.Repositories.Cart;
using TheLightStore.Infrastructure.Repositories.Category;
using TheLightStore.Infrastructure.Repositories.Payment;
using TheLightStore.Infrastructure.Repositories.InventoryTransaction;

namespace TheLightStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<FileStorageOptions>(options =>
        {
            options.WebRootPath = configuration["FileStorage:WebRootPath"] ?? "wwwroot";
        });
        
        services.Configure<GHNSettings>(configuration.GetSection("GHN"));

        // Database
        services.AddDbContext<DBContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(DBContext).Assembly.FullName)));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositories - Clean Architecture structure
        services.AddScoped<IOrderRepo, OrderRepo>();
        services.AddScoped<IOrderStatusHistoryRepo, OrderStatusHistoryRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        services.AddScoped<IProductVariantRepo, ProductVariantRepo>();
        services.AddScoped<IProductReviewRepo, ProductReviewRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<IShoppingCartRepo, ShoppingCartRepo>();
        services.AddScoped<ICartItemRepo, CartItemRepo>();
        services.AddScoped<ISavedCartRepo, SavedCartRepo>();
        services.AddScoped<IPaymentRepo, PaymentRepo>();
        services.AddScoped<IInventoryLogRepo, InventoryLogRepo>();
        services.AddScoped<IInventoryReservationRepo, InventoryReservationRepo>();
        services.AddScoped<IBannerRepository, BannerRepository>();
        services.AddScoped<IBlogRepository, BlogRepository>();
        
        // External Services
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        
        // GHN Service with HttpClient
        services.AddHttpClient<IGHNService, GHNService>();
        
        // SignalR
        services.AddSignalR();
        
        return services;
    }
}
