using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Application.Mappings;

public static class PowerMappings
{
    public static PowerGetDto EntityToGetDto(Power entity)
    {
        return new PowerGetDto
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

    public static Power CreateDtoToEntity(PowerCreateDto dto)
    {
        return new Power
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static Power UpdateDtoToEntity(PowerUpdateDto dto, Power entity)
    {
        entity.Name = dto.Name;
        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.UpdatedDate = DateTime.UtcNow;

        return entity;
    }
}
