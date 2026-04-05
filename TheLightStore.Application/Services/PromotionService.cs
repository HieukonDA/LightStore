using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;

    public PromotionService(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<ResponseResult> GetAll(PromotionDto.PromotionFilterParams filterParams)
    {
        try
        {
            var result = await _promotionRepository.GetAllAsync(filterParams);
            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var result = await _promotionRepository.GetByIdAsync(id);
        if (result == null)
            return new ErrorResponseResult("Không tìm thấy khuyến mãi");

        return new SuccessResponseResult(result);
    }

    public async Task<ResponseResult> Create(PromotionDto.PromotionCreateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            return new ErrorResponseResult("Tên khuyến mãi là bắt buộc");

        if (!string.IsNullOrWhiteSpace(createDto.Code))
        {
            var existingPromotion = await _promotionRepository.GetByCodeAsync(createDto.Code);
            if (existingPromotion != null)
                return new ErrorResponseResult("Mã khuyến mãi đã tồn tại");
        }

        var promotion = PromotionMappings.CreateDtoToEntity(createDto);
        promotion.CreatedDate = DateTime.UtcNow;
        promotion.IsActive = createDto.IsActive ?? true;

        var id = await _promotionRepository.CreateAsync(promotion);
        return new SuccessResponseResult(id);
    }

    public async Task<ResponseResult> Update(PromotionDto.PromotionUpdateDto updateDto)
    {
        if (updateDto.Id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var existingPromotion = await _promotionRepository.GetByIdAsync(updateDto.Id);
        if (existingPromotion == null)
            return new ErrorResponseResult("Không tìm thấy khuyến mãi");

        if (!string.IsNullOrWhiteSpace(updateDto.Code))
        {
            var promotionWithSameCode = await _promotionRepository.GetByCodeAsync(updateDto.Code);
            if (promotionWithSameCode != null && promotionWithSameCode.Id != updateDto.Id)
                return new ErrorResponseResult("Mã khuyến mãi đã tồn tại");
        }

        var promotion = await _promotionRepository.GetByIdAsync(updateDto.Id);
        var updatedPromotion = PromotionMappings.UpdateDtoToEntity(updateDto, new Promotion { Id = updateDto.Id });
        updatedPromotion.UpdatedDate = DateTime.UtcNow;

        var result = await _promotionRepository.UpdateAsync(updatedPromotion);
        return result 
            ? new SuccessResponseResult("Cập nhật khuyến mãi thành công")
            : new ErrorResponseResult("Cập nhật khuyến mãi thất bại");
    }

    public async Task<ResponseResult> Remove(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var exists = await _promotionRepository.ExistsAsync(id);
        if (!exists)
            return new ErrorResponseResult("Không tìm thấy khuyến mãi");

        var result = await _promotionRepository.DeleteAsync(id);
        return result 
            ? new SuccessResponseResult("Xóa khuyến mãi thành công")
            : new ErrorResponseResult("Xóa khuyến mãi thất bại");
    }

    public async Task<ResponseResult> GetProductsForGroupSelect(long promotionId)
    {
        if (promotionId <= 0)
            return new ErrorResponseResult("ID khuyến mãi phải lớn hơn 0");

        var exists = await _promotionRepository.ExistsAsync(promotionId);
        if (!exists)
            return new ErrorResponseResult("Không tìm thấy khuyến mãi");

        try
        {
            // TODO: Implement product selection for promotion group
            return new SuccessResponseResult(new List<object>());
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> GetValidPromotions()
    {
        try
        {
            var now = DateTime.UtcNow;
            var validPromotions = await _promotionRepository.GetAllActiveAsync();
            var filtered = validPromotions
                .Where(p => DateTime.Parse(p.StartedDate.ToString()) <= now && DateTime.Parse(p.EndedDate.ToString()) >= now)
                .ToList();

            return new SuccessResponseResult(filtered);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }
}
