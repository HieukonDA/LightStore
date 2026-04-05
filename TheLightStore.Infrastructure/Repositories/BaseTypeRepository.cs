using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Infrastructure.Repositories;

public class BaseTypeRepository : IBaseTypeRepository
{
    private readonly DBContext _context;

    public BaseTypeRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<BaseTypeGetDto>> GetAllAsync(BaseTypeFilterParams parameters)
    {
        var query = BuildQueryable(parameters);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(bt => MapToGetDto(bt))
            .ToListAsync();

        return new PaginationModel<BaseTypeGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, parameters.PageSize, parameters.PageNumber)
        };
    }

    public async Task<BaseTypeGetDto?> GetByIdAsync(long id)
    {
        var baseType = await _context.BaseTypes
            .Where(bt => bt.Id == id)
            .FirstOrDefaultAsync();

        return baseType == null ? null : MapToGetDto(baseType);
    }

    public async Task<BaseType?> GetByCodeAsync(string code)
    {
        return await _context.BaseTypes
            .Where(bt => bt.Code == code)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.BaseTypes.AnyAsync(bt => bt.Id == id);
    }

    public async Task<long> CreateAsync(BaseType entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsActive = true;

        _context.BaseTypes.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(BaseType entity)
    {
        var existing = await _context.BaseTypes.FindAsync(entity.Id);
        if (existing == null)
            return false;

        existing.Name = entity.Name;
        existing.Code = entity.Code;
        existing.Description = entity.Description;
        existing.IsActive = entity.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;

        _context.BaseTypes.Update(existing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var baseType = await _context.BaseTypes.FindAsync(id);
        if (baseType == null)
            return false;

        _context.BaseTypes.Remove(baseType);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<BaseTypeGetDto>> GetAllActiveAsync()
    {
        return await _context.BaseTypes
            .Where(bt => bt.IsActive)
            .OrderBy(bt => bt.Name)
            .Select(bt => MapToGetDto(bt))
            .ToListAsync();
    }

    private IQueryable<BaseType> BuildQueryable(BaseTypeFilterParams parameters)
    {
        var query = _context.BaseTypes.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Code))
            query = query.Where(bt => bt.Code.Contains(parameters.Code));

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(bt => bt.Name.Contains(parameters.Name));

        if (parameters.IsActive.HasValue)
            query = query.Where(bt => bt.IsActive == parameters.IsActive.Value);

        if (!string.IsNullOrEmpty(parameters.CreatedBy))
            query = query.Where(bt => bt.CreatedBy == parameters.CreatedBy);

        return query.OrderByDescending(bt => bt.CreatedDate);
    }

    private static BaseTypeGetDto MapToGetDto(BaseType baseType)
    {
        return new BaseTypeGetDto
        {
            Id = baseType.Id,
            Name = baseType.Name,
            Code = baseType.Code,
            Description = baseType.Description,
            IsActive = baseType.IsActive,
            CreatedDate = baseType.CreatedDate,
            CreatedBy = baseType.CreatedBy,
            UpdatedDate = baseType.UpdatedDate,
            UpdatedBy = baseType.UpdatedBy
        };
    }
}
