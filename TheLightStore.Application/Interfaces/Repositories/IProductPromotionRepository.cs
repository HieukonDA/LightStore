using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ProductPromotionDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IProductPromotionRepository
{
    Task<PaginationModel<ProductPromotionGetDto>> GetAllAsync(ProductPromotionFilterParams filterParams);
    Task<ProductPromotionGetDto?> GetByIdAsync(long id);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(ProductPromotion productPromotion);
    Task<bool> UpdateAsync(ProductPromotion productPromotion);
    Task<bool> DeleteAsync(long id);
    Task<List<ProductPromotionGetDto>> GetAllActiveAsync();
    Task<List<ProductsByPromotionDto>> GetProductsByPromotionIdAsync(long promotionId);
}
