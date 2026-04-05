using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.BrandDto;

namespace TheLightStore.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly DBContext _context;

    public BrandRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<BrandGetDto>> GetAllAsync(BrandFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(b => MapToGetDto(b))
            .ToListAsync();

        return new PaginationModel<BrandGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<BrandGetDto?> GetByIdAsync(long id)
    {
        var brand = await _context.Brands
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync();

        return brand == null ? null : MapToGetDto(brand);
    }

    public async Task<Brand?> GetByCodeAsync(string code)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Code == code);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Brands.AnyAsync(b => b.Id == id);
    }

    public async Task<long> CreateAsync(Brand brand)
    {
        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();
        return brand.Id;
    }

    public async Task<bool> UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand == null)
            return false;

        _context.Brands.Remove(brand);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<BrandGetDto>> GetAllActiveAsync()
    {
        return await _context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => MapToGetDto(b))
            .ToListAsync();
    }

    private IQueryable<Brand> BuildQueryable(BrandFilterParams filterParams)
    {
        var query = _context.Brands.AsQueryable();

        if (!string.IsNullOrEmpty(filterParams.Code))
            query = query.Where(b => b.Code.Contains(filterParams.Code));

        if (!string.IsNullOrEmpty(filterParams.Name))
            query = query.Where(b => b.Name.Contains(filterParams.Name));

        if (filterParams.IsActive.HasValue)
            query = query.Where(b => b.IsActive == filterParams.IsActive.Value);

        if (!string.IsNullOrEmpty(filterParams.CreatedBy))
            query = query.Where(b => b.CreatedBy == filterParams.CreatedBy);

        return query.OrderByDescending(b => b.CreatedDate);
    }

    private static BrandGetDto MapToGetDto(Brand brand)
    {
        return new BrandGetDto
        {
            Id = brand.Id,
            Code = brand.Code,
            Name = brand.Name,
            Description = brand.Description,
            IsActive = brand.IsActive,
            CreatedDate = brand.CreatedDate,
            CreatedBy = brand.CreatedBy,
            UpdatedDate = brand.UpdatedDate,
            UpdatedBy = brand.UpdatedBy
        };
    }
}
