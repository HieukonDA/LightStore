using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class ProductPromotionMappings
{
    public static ProductPromotionDto.ProductPromotionGetDto EntityToGetDto(ProductPromotion entity)
    {
        return new ProductPromotionDto.ProductPromotionGetDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            PromotionId = entity.PromotionId,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static ProductPromotion CreateDtoToEntity(ProductPromotionDto.ProductPromotionCreateDto dto)
    {
        return new ProductPromotion
        {
            ProductId = dto.ProductId,
            PromotionId = dto.PromotionId,
            IsActive = dto.IsActive ?? true
        };
    }

    public static ProductPromotion UpdateDtoToEntity(ProductPromotionDto.ProductPromotionUpdateDto dto, ProductPromotion entity)
    {
        entity.ProductId = dto.ProductId;
        entity.PromotionId = dto.PromotionId;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        return entity;
    }
}
