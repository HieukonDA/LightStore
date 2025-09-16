namespace TheLightStore.Interfaces.Repository;

using Microsoft.EntityFrameworkCore.Storage;
using TheLightStore.Dtos.Product;
using TheLightStore.Models.Products;
public interface IProductRepo
{
    // Basic CRUD
    Task<Product?> GetByIdAsync(int id, bool includeRelated = false);
    Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false);
    Task<PagedResult<Product>> GetAllAsync(PagedRequest request);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);

    // // Specialized Queries
    // Task<IEnumerable<Product>> GetFeaturedAsync(int count = 10);
    // Task<IEnumerable<Product>> GetNewProductsAsync(int count = 10);
    Task<PagedResult<Product>> GetByCategoryAsync(int categoryId, PagedRequest pagedRequest);
    // Task<IEnumerable<Product>> GetByBrandAsync(int brandId, int count = 20);
    // Task<IEnumerable<Product>> GetRelatedAsync(int productId, int count = 5);
    // Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids);

    // // Stock Management
    // Task<bool> UpdateStockAsync(int productId, int quantity);
    // Task<bool> ReserveStockAsync(int productId, int quantity);
    // Task<bool> ReleaseStockAsync(int productId, int quantity);
    // Task<IEnumerable<Product>> GetLowStockAsync();
    // Task<IEnumerable<Product>> GetOutOfStockAsync();

    // // Analytics
    // Task IncrementViewCountAsync(int productId);
    // Task<IEnumerable<Product>> GetMostViewedAsync(int count = 10);
    // Task<IEnumerable<Product>> GetTopRatedAsync(int count = 10);

    Task<Dictionary<int, ProductAvailabilityInfo>> GetProductsAvailabilityWithLockAsync(
        List<int> productIds, IDbContextTransaction transaction);
}