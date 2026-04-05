using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class ProductDetailMappings
{
    public static ProductDetailDto.ProductDetailGetDto EntityToGetDto(ProductDetail entity)
    {
        return new ProductDetailDto.ProductDetailGetDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            PowerId = entity.PowerId,
            ColorTemperatureId = entity.ColorTemperatureId,
            ShapeId = entity.ShapeId,
            BaseTypeId = entity.BaseTypeId,
            CurrencyId = entity.CurrencyId,
            SellingPrice = entity.SellingPrice,
            EarningPoints = entity.EarningPoints,
            SoldQuantity = entity.SoldQuantity,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static ProductDetail CreateDtoToEntity(ProductDetailDto.ProductDetailCreateDto dto)
    {
        return new ProductDetail
        {
            ProductId = dto.ProductId,
            PowerId = dto.PowerId,
            ColorTemperatureId = dto.ColorTemperatureId,
            ShapeId = dto.ShapeId,
            BaseTypeId = dto.BaseTypeId,
            CurrencyId = dto.CurrencyId,
            SellingPrice = dto.SellingPrice,
            EarningPoints = dto.EarningPoints,
            SoldQuantity = dto.SoldQuantity,
            Description = dto.Description,
            IsActive = dto.IsActive ?? false
        };
    }

    public static ProductDetail UpdateDtoToEntity(ProductDetailDto.ProductDetailUpdateDto dto, ProductDetail entity)
    {
        entity.SellingPrice = dto.SellingPrice;
        entity.EarningPoints = dto.EarningPoints;
        entity.SoldQuantity = dto.SoldQuantity;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;
        return entity;
    }

    public static ProductDetailDto.RemainingProductQuantity RemainingProductQuantity(ProductDetail entity)
    {
        return new ProductDetailDto.RemainingProductQuantity
        {
            SoldQuantity = entity.SoldQuantity,
            RemainingQuantity = entity.SoldQuantity.HasValue ? (1000 - entity.SoldQuantity.Value) : 1000
        };
    }
}
