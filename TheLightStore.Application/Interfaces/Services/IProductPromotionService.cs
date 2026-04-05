using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface IProductPromotionService
{
    Task<ResponseResult> GetAll(ProductPromotionDto.ProductPromotionFilterParams filterParams);
    Task<ResponseResult> GetProductsByPromotionId(long promotionId);
    Task<ResponseResult> Create(ProductPromotionDto.ProductPromotionCreateDto createDto);
    Task<ResponseResult> CreateMultiple(ProductPromotionDto.ListProductPromotionCreateDto createDto);
    Task<ResponseResult> UpdateMultiple(ProductPromotionDto.ListProductPromotionUpdateDto updateDto);
    Task<ResponseResult> RemoveMultiple(ProductPromotionDto.ListProductPromotionRemoveDto removeDto);
}
