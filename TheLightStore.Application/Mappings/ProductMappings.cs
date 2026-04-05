using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class ProductMappings
{
    public static ProductDto.ProductGetDto EntityToGetDto(Product entity)
    {
        return new ProductDto.ProductGetDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ProductType = entity.ProductType,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            BrandId = entity.BrandId,
            IsInBusiness = entity.IsInBusiness,
            IsOrderedOnline = entity.IsOrderedOnline,
            IsPackaged = entity.IsPackaged,
            Description = entity.Description,
            Position = entity.Position,
            ImageUrl = entity.ImageUrl,
            IsActive = entity.IsActive,
            AverageRatingPoint = entity.AverageRatingPoint,
            TotalSoldQuantity = entity.TotalSoldQuantity,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Product CreateDtoToEntity(ProductDto.ProductCreateDto dto)
    {
        return new Product
        {
            Code = dto.Code,
            ProductType = dto.ProductType ?? "SELF_PRODUCED",
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            IsInBusiness = dto.IsInBusiness,
            IsOrderedOnline = dto.IsOrderedOnline,
            IsPackaged = dto.IsPackaged,
            Description = dto.Description,
            Position = dto.Position,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive ?? true,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static Product UpdateDtoToEntity(ProductDto.ProductUpdateDto dto, Product entity)
    {
        entity.Code = dto.Code;
        entity.ProductType = dto.ProductType ?? "SELF_PRODUCED";
        entity.Name = dto.Name;
        entity.CategoryId = dto.CategoryId;
        entity.BrandId = dto.BrandId;
        entity.IsInBusiness = dto.IsInBusiness;
        entity.IsOrderedOnline = dto.IsOrderedOnline;
        entity.IsPackaged = dto.IsPackaged;
        entity.Description = dto.Description;
        entity.Position = dto.Position;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;
        return entity;
    }

    public static Product UpdateExtraDtoToEntity(ProductDto.UpdateExtraDto dto, Product entity)
    {
        entity.Code = dto.Code;
        entity.ProductType = dto.ProductType ?? "SELF_PRODUCED";
        entity.Name = dto.Name;
        entity.CategoryId = dto.CategoryId;
        entity.BrandId = dto.BrandId;
        entity.IsInBusiness = dto.IsInBusiness;
        entity.IsOrderedOnline = dto.IsOrderedOnline;
        entity.IsPackaged = dto.IsPackaged;
        entity.Description = dto.Description;
        entity.Position = dto.Position;
        entity.ImageUrl = dto.ImageUrl;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;
        return entity;
    }
}
