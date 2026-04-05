using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Application.Mappings;

public static class ShapeMappings
{
    public static ShapeGetDto EntityToGetDto(Shape entity)
    {
        return new ShapeGetDto
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

    public static Shape CreateDtoToEntity(ShapeCreateDto dto)
    {
        return new Shape
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static Shape UpdateDtoToEntity(ShapeUpdateDto dto, Shape entity)
    {
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        return entity;
    }
}
