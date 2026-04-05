using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Infrastructure.Repositories;

public class ShapeRepository : IShapeRepository
{
    private readonly DBContext _context;

    public ShapeRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<ShapeGetDto>> GetAllAsync(ShapeFilterParams parameters)
    {
        var query = BuildQueryable(parameters);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(s => MapToGetDto(s))
            .ToListAsync();

        return new PaginationModel<ShapeGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, parameters.PageSize, parameters.PageNumber)
        };
    }

    public async Task<ShapeGetDto?> GetByIdAsync(long id)
    {
        var shape = await _context.Shapes
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();

        return shape == null ? null : MapToGetDto(shape);
    }

    public async Task<Shape?> GetByCodeAsync(string code)
    {
        return await _context.Shapes
            .Where(s => s.Code == code)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Shapes.AnyAsync(s => s.Id == id);
    }

    public async Task<long> CreateAsync(Shape entity)
    {
        entity.CreatedDate = DateTime.UtcNow;
        entity.IsActive = true;

        _context.Shapes.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Shape entity)
    {
        var existing = await _context.Shapes.FindAsync(entity.Id);
        if (existing == null)
            return false;

        existing.Name = entity.Name;
        existing.Code = entity.Code;
        existing.Description = entity.Description;
        existing.IsActive = entity.IsActive;
        existing.UpdatedDate = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;

        _context.Shapes.Update(existing);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var shape = await _context.Shapes.FindAsync(id);
        if (shape == null)
            return false;

        _context.Shapes.Remove(shape);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ShapeGetDto>> GetAllActiveAsync()
    {
        return await _context.Shapes
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => MapToGetDto(s))
            .ToListAsync();
    }

    private IQueryable<Shape> BuildQueryable(ShapeFilterParams parameters)
    {
        var query = _context.Shapes.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Code))
            query = query.Where(s => s.Code.Contains(parameters.Code));

        if (!string.IsNullOrEmpty(parameters.Name))
            query = query.Where(s => s.Name.Contains(parameters.Name));

        if (parameters.IsActive.HasValue)
            query = query.Where(s => s.IsActive == parameters.IsActive.Value);

        if (!string.IsNullOrEmpty(parameters.CreatedBy))
            query = query.Where(s => s.CreatedBy == parameters.CreatedBy);

        return query.OrderByDescending(s => s.CreatedDate);
    }

    private static ShapeGetDto MapToGetDto(Shape shape)
    {
        return new ShapeGetDto
        {
            Id = shape.Id,
            Name = shape.Name,
            Code = shape.Code,
            Description = shape.Description,
            IsActive = shape.IsActive,
            CreatedDate = shape.CreatedDate,
            CreatedBy = shape.CreatedBy,
            UpdatedDate = shape.UpdatedDate,
            UpdatedBy = shape.UpdatedBy
        };
    }
}
