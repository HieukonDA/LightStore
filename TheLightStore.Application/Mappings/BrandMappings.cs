using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class BrandMappings
{
    public static BrandDto.BrandGetDto EntityToGetDto(Brand entity)
    {
        return new BrandDto.BrandGetDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Brand CreateDtoToEntity(BrandDto.BrandCreateDto dto)
    {
        return new Brand
        {
            Code = string.Empty,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static Brand UpdateDtoToEntity(BrandDto.BrandUpdateDto dto, Brand entity)
    {
        entity.Code = string.IsNullOrWhiteSpace(entity.Code) ? string.Empty : entity.Code;
        entity.Name = dto.Name ?? entity.Name;
        entity.Description = dto.Description ?? entity.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        return entity;
    }
}
