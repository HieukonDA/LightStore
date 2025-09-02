using TheLightStore.Dtos.Products;
using TheLightStore.Interfaces.Products;

namespace TheLightStore.Interfaces.Products;

public interface IProductService
{
    // Product Management
    Task<ServiceResult<ProductDto>> GetByIdAsync(int id);
    Task<ServiceResult<ProductDto>> GetBySlugAsync(string slug);
    Task<ServiceResult<PagedResult<ProductListDto>>> GetPagedAsync(PagedRequest pagedRequest);
    
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto);
    Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    
    // // Product Collections
    // Task<ServiceResult<List<ProductListDto>>> GetFeaturedAsync(int count = 10);
    // Task<ServiceResult<List<ProductListDto>>> GetNewProductsAsync(int count = 10);
    Task<ServiceResult<PagedResult<ProductListDto>>> GetByCategoryAsync(int categoryId, PagedRequest pagedRequest);
    // Task<ServiceResult<List<ProductListDto>>> GetRelatedAsync(int productId, int count = 5);
    
    // // Stock & Inventory
    // Task<ServiceResult<bool>> UpdateStockAsync(int productId, int quantity);
    // Task<ServiceResult<InventoryStatusDto>> CheckStockAsync(int productId, int variantId = 0);
    // Task<ServiceResult<bool>> ReserveStockAsync(int productId, int quantity, int variantId = 0);
    
    // // Business Operations
    // Task<ServiceResult<bool>> ToggleFeaturedAsync(int productId);
    // Task<ServiceResult<bool>> UpdateStatusAsync(int productId, ProductStatus status);
    // Task<ServiceResult<decimal>> CalculateFinalPriceAsync(int productId, int variantId = 0);
    
    // // Analytics
    // Task<ServiceResult<bool>> IncrementViewAsync(int productId);
    // Task<ServiceResult<ProductStatsDto>> GetStatsAsync(int productId);
}