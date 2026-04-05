using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Infrastructure.Repositories;

public class ColorTemperatureRepository : IColorTemperatureRepository
{
    private readonly DBContext _context;

    public ColorTemperatureRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<ColorTemperatureGetDto>> GetAllAsync(ColorTemperatureFilterParams parameters)
    {
        var query = BuildQueryable(parameters);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(ct => MapToGetDto(ct))
            .ToListAsync();

        return new PaginationModel<ColorTemperatureGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, parameters.PageSize, parameters.PageNumber)
        };
    }

    public async Task<ColorTemperatureGetDto?> GetByIdAsync(long id)
    {
        var colorTemperature = await _context.ColorTemperatures
            .Where(ct => ct.Id == id)
            .FirstOrDefaultAsync();

        return colorTemperature == null ? null : MapToGetDto(colorTemperature);
    }

    public async Task<ColorTemperature?> GetByCodeAsync(string code)
    {
        return await _context.ColorTemperatures
            .Where(ct => ct.Code == code)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.ColorTemperatures.AnyAsync(ct => ct.Id == id);
    }

    public async Task<long> CreateAsync(ColorTemperature entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsActive = true;

        _context.ColorTemperatures.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(ColorTemperature entity)
    {
        var existing = await _context.ColorTemperatures.FindAsync(entity.Id);
        if (existing == null)
            return false;

        existing.Name = entity.Name;
        existing.Code = entity.Code;
        existing.Description = entity.Description;
        existing.IsActive = entity.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;

        _context.ColorTemperatures.Update(existing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var colorTemperature = await _context.ColorTemperatures.FindAsync(id);
        if (colorTemperature == null)
            return false;

        _context.ColorTemperatures.Remove(colorTemperature);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ColorTemperatureGetDto>> GetAllActiveAsync()
    {
        return await _context.ColorTemperatures
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToGetDto(ct))
            .ToListAsync();
    }

    private IQueryable<ColorTemperature> BuildQueryable(ColorTemperatureFilterParams parameters)
    {
        var query = _context.ColorTemperatures.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Code))
            query = query.Where(ct => ct.Code.Contains(parameters.Code));

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(ct => ct.Name.Contains(parameters.Name));

        if (parameters.IsActive.HasValue)
            query = query.Where(ct => ct.IsActive == parameters.IsActive.Value);

        if (!string.IsNullOrEmpty(parameters.CreatedBy))
            query = query.Where(ct => ct.CreatedBy == parameters.CreatedBy);

        return query.OrderByDescending(ct => ct.CreatedDate);
    }

    private static ColorTemperatureGetDto MapToGetDto(ColorTemperature colorTemperature)
    {
        return new ColorTemperatureGetDto
        {
            Id = colorTemperature.Id,
            Name = colorTemperature.Name,
            Code = colorTemperature.Code,
            Description = colorTemperature.Description,
            IsActive = colorTemperature.IsActive,
            CreatedDate = colorTemperature.CreatedDate,
            CreatedBy = colorTemperature.CreatedBy,
            UpdatedDate = colorTemperature.UpdatedDate,
            UpdatedBy = colorTemperature.UpdatedBy
        };
    }
}
