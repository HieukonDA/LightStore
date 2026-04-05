using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Application.Mappings;

public static class ColorTemperatureMappings
{
    public static ColorTemperatureGetDto EntityToGetDto(ColorTemperature entity)
    {
        return new ColorTemperatureGetDto
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

    public static ColorTemperature CreateDtoToEntity(ColorTemperatureCreateDto dto)
    {
        return new ColorTemperature
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static ColorTemperature UpdateDtoToEntity(ColorTemperatureUpdateDto dto, ColorTemperature entity)
    {
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        return entity;
    }
}
