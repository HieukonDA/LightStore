using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Mappings;

public static class PromotionMappings
{
    public static PromotionDto.PromotionGetDto EntityToGetDto(Promotion entity)
    {
        return new PromotionDto.PromotionGetDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            PercentDiscount = entity.PercentDiscount,
            StartedDate = entity.StartedDate,
            EndedDate = entity.EndedDate,
            Status = entity.Status,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static Promotion CreateDtoToEntity(PromotionDto.PromotionCreateDto dto)
    {
        return new Promotion
        {
            Code = dto.Code ?? string.Empty,
            Name = dto.Name,
            PercentDiscount = dto.PercentDiscount,
            StartedDate = dto.StartedDate,
            EndedDate = dto.EndedDate,
            Status = dto.Status,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };
    }

    public static Promotion UpdateDtoToEntity(PromotionDto.PromotionUpdateDto dto, Promotion entity)
    {
        entity.Code = dto.Code ?? entity.Code;
        entity.Name = dto.Name ?? entity.Name;
        entity.PercentDiscount = dto.PercentDiscount;
        entity.StartedDate = dto.StartedDate;
        entity.EndedDate = dto.EndedDate;
        entity.Status = dto.Status ?? entity.Status;
        entity.Description = dto.Description ?? entity.Description;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        return entity;
    }
}
