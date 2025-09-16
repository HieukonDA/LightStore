using Microsoft.EntityFrameworkCore.Storage;
using TheLightStore.Dtos.Product;

namespace TheLightStore.Interfaces.Repository;

public interface IProductVariantRepo
{
    // Basic CRUD
    Task<ProductVariant?> GetByIdAsync(int id);
    Task<ProductVariant?> GetByProductIdAsync(int productId);
    Task<ProductVariant?> GetBySkuAsync(string sku);
    Task<ProductVariant> AddAsync(ProductVariant productVariant);
    Task<ProductVariant> UpdateAsync(ProductVariant productVariant);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStockAsync(int variantId, int quantity);
    Task<Dictionary<int, ProductAvailabilityInfo>> GetVariantsAvailabilityWithLockAsync(
        List<int> variantIds,
        IDbContextTransaction transaction);
}