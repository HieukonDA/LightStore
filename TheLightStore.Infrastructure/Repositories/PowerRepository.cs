using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Infrastructure.Repositories;

public class PowerRepository : IPowerRepository
{
    private readonly DBContext _context;

    public PowerRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<PowerGetDto>> GetAllAsync(PowerFilterParams parameters)
    {
        var query = BuildQueryable(parameters);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => MapToGetDto(p))
            .ToListAsync();

        return new PaginationModel<PowerGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, parameters.PageSize, parameters.PageNumber)
        };
    }

    public async Task<PowerGetDto?> GetByIdAsync(long id)
    {
        var power = await _context.Powers
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        return power == null ? null : MapToGetDto(power);
    }

    public async Task<Power?> GetByCodeAsync(string code)
    {
        return await _context.Powers
            .Where(p => p.Code == code)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Powers.AnyAsync(p => p.Id == id);
    }

    public async Task<long> CreateAsync(Power entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsActive = true;

        _context.Powers.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Power entity)
    {
        var existing = await _context.Powers.FindAsync(entity.Id);
        if (existing == null)
            return false;

        existing.Name = entity.Name;
        existing.Code = entity.Code;
        existing.Description = entity.Description;
        existing.IsActive = entity.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;

        _context.Powers.Update(existing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var power = await _context.Powers.FindAsync(id);
        if (power == null)
            return false;

        _context.Powers.Remove(power);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PowerGetDto>> GetAllActiveAsync()
    {
        return await _context.Powers
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => MapToGetDto(p))
            .ToListAsync();
    }

    private IQueryable<Power> BuildQueryable(PowerFilterParams parameters)
    {
        var query = _context.Powers.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Code))
            query = query.Where(p => p.Code.Contains(parameters.Code));

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(p => p.Name.Contains(parameters.Name));

        if (parameters.IsActive.HasValue)
            query = query.Where(p => p.IsActive == parameters.IsActive.Value);

        if (!string.IsNullOrEmpty(parameters.CreatedBy))
            query = query.Where(p => p.CreatedBy == parameters.CreatedBy);

        return query.OrderByDescending(p => p.CreatedDate);
    }

    private static PowerGetDto MapToGetDto(Power power)
    {
        return new PowerGetDto
        {
            Id = power.Id,
            Name = power.Name,
            Code = power.Code,
            Description = power.Description,
            IsActive = power.IsActive,
            CreatedDate = power.CreatedDate,
            CreatedBy = power.CreatedBy,
            UpdatedDate = power.UpdatedDate,
            UpdatedBy = power.UpdatedBy
        };
    }
}

