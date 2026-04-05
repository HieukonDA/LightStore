using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Application.Mappings;

public static class BaseTypeMappings
{
    public static BaseTypeGetDto EntityToGetDto(BaseType entity)
    {
        return new BaseTypeGetDto
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code ?? string.Empty,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static BaseType CreateDtoToEntity(BaseTypeCreateDto dto)
    {
        return new BaseType
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static BaseType UpdateDtoToEntity(BaseTypeUpdateDto dto, BaseType entity)
    {
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        return entity;
    }
}
