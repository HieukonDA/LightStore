using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class CategoryMappings
{
    public static CategoryDto.CategoryGetDto EntityToGetDto(Category entity)
    {
        return new CategoryDto.CategoryGetDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ParentId = entity.ParentId,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Category CreateDtoToEntity(CategoryDto.CategoryCreateDto dto)
    {
        return new Category
        {
            Code = dto.Code ?? string.Empty,
            Name = dto.Name,
            ParentId = dto.ParentId,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static Category UpdateDtoToEntity(CategoryDto.CategoryUpdateDto dto, Category entity)
    {
        entity.Code = dto.Code ?? entity.Code;
        entity.Name = dto.Name ?? entity.Name;
        entity.ParentId = dto.ParentId ?? entity.ParentId;
        entity.Description = dto.Description ?? entity.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        return entity;
    }
}
