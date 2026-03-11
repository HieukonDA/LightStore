
using TheLightStore.Application.DTOs.Paging;
using TheLightStore.Application.DTOs.Products;
using TheLightStore.Application.DTOs.Search;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IProductRepo
{
    // Basic CRUD - User methods (chỉ lấy active)
    Task<Product?> GetByIdAsync(int id, bool includeRelated = false);
    Task<Product?> GetBySlugAsync(string slug, bool includeRelated = false);
    Task<PagedResult<Product>> GetAllAsync(PagedRequest request);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);

    // 🔥 Admin methods - lấy tất cả (bao gồm inactive)
    Task<Product?> GetByIdForAdminAsync(int id, bool includeRelated = false);
    Task<PagedResult<Product>> GetAllForAdminAsync(PagedRequest request);
    Task<bool> ExistsForAdminAsync(int id);
    Task<bool> SlugExistsForAdminAsync(string slug, int? excludeId = null);

    // Specialized Queries
    Task<PagedResult<Product>> GetFeaturedAsync(PagedRequest pagedRequest);
    Task<PagedResult<Product>> GetNewProductsAsync(PagedRequest pagedRequest);
    Task<PagedResult<Product>> GetByCategoryAsync(int categoryId, PagedRequest pagedRequest);
    Task<PagedResult<Product>> GetByCategorySlugAsync(string slug, PagedRequest pagedRequest);
    Task<PagedResult<Product>> GetRelatedAsync(int productId, PagedRequest pagedRequest);

    Task<Dictionary<int, ProductAvailabilityInfo>> GetProductsAvailabilityWithLockAsync(
        List<int> productIds, ITransaction transaction);
 
    // Search methods
    Task<PagedResult<Product>> SearchAsync(SearchProductsRequest request);
    Task<List<Product>> GetProductSuggestionsAsync(string query, int limit = 10);
    Task<Dictionary<string, int>> GetSearchFiltersAsync(string query);
}
