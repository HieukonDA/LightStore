using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Application.Interfaces.Services;

public interface IPromotionService
{
    Task<ResponseResult> GetAll(PromotionDto.PromotionFilterParams filterParams);
    Task<ResponseResult> GetById(long id);
    Task<ResponseResult> Create(PromotionDto.PromotionCreateDto createDto);
    Task<ResponseResult> Update(PromotionDto.PromotionUpdateDto updateDto);
    Task<ResponseResult> Remove(long id);
    Task<ResponseResult> GetProductsForGroupSelect(long promotionId);
    Task<ResponseResult> GetValidPromotions();
}
