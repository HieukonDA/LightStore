using System;
using TheLightStore.Application.DTOs;
using TheLightStore.Application.DTOs.Products;

namespace TheLightStore.Application.Interfaces;

public interface IProductVariantService
{
    // Basic CRUD - User methods (chỉ lấy active)
    Task<ServiceResult<ProductVariantDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<ProductVariantDto>>> GetByProductIdAsync(int productId);
    Task<ServiceResult<ProductVariantDto>> GetBySkuAsync(string sku);
    Task<ServiceResult<ProductVariantDto>> CreateAsync(int productId, CreateProductVariantDto dto);
    Task<ServiceResult<ProductVariantDto>> UpdateAsync(int id, UpdateProductVariantDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);

    // 🔥 Admin methods - lấy tất cả (bao gồm inactive)
    Task<ServiceResult<ProductVariantDto>> GetByIdForAdminAsync(int id);
    Task<ServiceResult<List<ProductVariantDto>>> GetByProductIdForAdminAsync(int productId);

    // Stock Management
    Task<ServiceResult<bool>> UpdateStockAsync(int variantId, int quantity);
    Task<ServiceResult<StockStatusDto>> CheckStockAsync(int variantId);
    Task<ServiceResult<bool>> ReserveStockAsync(int variantId, int quantity);
    Task<ServiceResult<bool>> ReleaseStockAsync(int variantId, int quantity);

    // Attribute Management
    Task<ServiceResult<bool>> UpdateAttributesAsync(int variantId, List<ProductVariantAttributeDto> attributes);
    Task<ServiceResult<List<ProductVariantAttributeDto>>> GetAttributesAsync(int variantId);

    // SKU Management
    Task<ServiceResult<string>> GenerateSkuAsync(int productId, List<ProductVariantAttributeDto> attributes);
    Task<ServiceResult<bool>> ValidateSkuAsync(string sku, int? excludeVariantId = null);

    // Business Operations
    Task<ServiceResult<bool>> ToggleActiveStatusAsync(int variantId);
    Task<ServiceResult<bool>> UpdateSortOrderAsync(int variantId, int sortOrder);
    Task<ServiceResult<decimal>> CalculateFinalPriceAsync(int variantId);

    // Inventory Analytics
    Task<ServiceResult<List<ProductVariantDto>>> GetLowStockVariantsAsync(int? productId = null);
    Task<ServiceResult<List<ProductVariantDto>>> GetOutOfStockVariantsAsync(int? productId = null);

    // Image Management
    Task<ServiceResult<bool>> AssignImageAsync(int variantId, int imageId);
    Task<ServiceResult<bool>> RemoveImageAsync(int variantId, int imageId);
}