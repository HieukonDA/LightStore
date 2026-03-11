using TheLightStore.Application.DTOs.Products;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IProductVariantRepo
{
    // Basic CRUD - User methods (chỉ lấy active)
    Task<ProductVariants?> GetByIdAsync(int id, bool includeRelated = false);
    Task<List<ProductVariants>> GetByProductIdAsync(int productId, bool includeRelated = false);
    Task<ProductVariants?> GetBySkuAsync(string sku, bool includeRelated = false);
    Task<ProductVariants> AddAsync(ProductVariants productVariant);
    Task<ProductVariants> UpdateAsync(ProductVariants productVariant);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);

    // 🔥 Admin methods - lấy tất cả (bao gồm inactive)
    Task<ProductVariants?> GetByIdForAdminAsync(int id, bool includeRelated = false);
    Task<List<ProductVariants>> GetByProductIdForAdminAsync(int productId, bool includeRelated = false);
    Task<bool> ExistsForAdminAsync(int id);
    Task<bool> SkuExistsForAdminAsync(string sku, int? excludeId = null);

    // Stock Management
    Task<bool> UpdateStockAsync(int variantId, int quantity);
    Task<bool> ReserveStockAsync(int variantId, int quantity);
    Task<bool> ReleaseStockAsync(int variantId, int quantity);
    Task<Dictionary<int, ProductAvailabilityInfo>> GetVariantsAvailabilityWithLockAsync(
        List<int> variantIds,
        ITransaction transaction);

    // Attribute Management
    Task<List<ProductVariantAttribute>> GetVariantAttributesAsync(int variantId);
    Task<bool> UpdateVariantAttributesAsync(int variantId, List<ProductVariantAttribute> attributes);

    // Analytics & Filtering
    Task<List<ProductVariants>> GetLowStockVariantsAsync(int? productId = null);
    Task<List<ProductVariants>> GetOutOfStockVariantsAsync(int? productId = null);
    Task<List<ProductVariants>> GetActiveVariantsAsync(int? productId = null);

    // Sorting
    Task<bool> UpdateSortOrderAsync(int variantId, int sortOrder);
    Task<List<ProductVariants>> GetVariantsByProductOrderedAsync(int productId);
}
