using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.CategoryDto;

namespace TheLightStore.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DBContext _context;

    public CategoryRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<CategoryGetDto>> GetAllAsync(CategoryFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(c => MapToGetDto(c))
            .ToListAsync();

        return new PaginationModel<CategoryGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<CategoryGetDto?> GetByIdAsync(long id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        return category == null ? null : MapToGetDto(category);
    }

    public async Task<Category?> GetByCodeAsync(string code)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<long> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<CategoryGetDto>> GetAllActiveAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => MapToGetDto(c))
            .ToListAsync();
    }

    private IQueryable<Category> BuildQueryable(CategoryFilterParams filterParams)
    {
        var query = _context.Categories.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(filterParams.Code))
            query = query.Where(x => x.Code.Contains(filterParams.Code));

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
            query = query.Where(x => x.Name.Contains(filterParams.Name));

        if (filterParams.ParentId.HasValue)
            query = query.Where(x => x.ParentId == filterParams.ParentId);

        if (!string.IsNullOrWhiteSpace(filterParams.CreatedBy))
            query = query.Where(x => x.CreatedBy == filterParams.CreatedBy);

        return query.OrderBy(x => x.Name);
    }

    private static CategoryGetDto MapToGetDto(Category category)
    {
        return new CategoryGetDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            ParentId = category.ParentId,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedDate = category.CreatedDate,
            CreatedBy = category.CreatedBy,
            UpdatedDate = category.UpdatedDate,
            UpdatedBy = category.UpdatedBy
        };
    }
}
