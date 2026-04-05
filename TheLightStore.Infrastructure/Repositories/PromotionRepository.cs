using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.PromotionDto;

namespace TheLightStore.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly DBContext _context;

    public PromotionRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<PromotionGetDto>> GetAllAsync(PromotionFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(p => MapToGetDto(p))
            .ToListAsync();

        return new PaginationModel<PromotionGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<PromotionGetDto?> GetByIdAsync(long id)
    {
        var promotion = await _context.Promotions
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

        return promotion == null ? null : MapToGetDto(promotion);
    }

    public async Task<Promotion?> GetByCodeAsync(string code)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Promotions.AnyAsync(p => p.Id == id);
    }

    public async Task<long> CreateAsync(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion.Id;
    }

    public async Task<bool> UpdateAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
            return false;

        _context.Promotions.Remove(promotion);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<PromotionGetDto>> GetAllActiveAsync()
    {
        return await _context.Promotions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => MapToGetDto(p))
            .ToListAsync();
    }

    private IQueryable<Promotion> BuildQueryable(PromotionFilterParams filterParams)
    {
        var query = _context.Promotions.AsQueryable();

        if (!string.IsNullOrEmpty(filterParams.Code))
            query = query.Where(p => p.Code.Contains(filterParams.Code));

        if (!string.IsNullOrEmpty(filterParams.Name))
            query = query.Where(p => p.Name.Contains(filterParams.Name));

        if (!string.IsNullOrEmpty(filterParams.Status))
            query = query.Where(p => p.Status == filterParams.Status);

        if (filterParams.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filterParams.IsActive.Value);

        if (!string.IsNullOrEmpty(filterParams.CreatedBy))
            query = query.Where(p => p.CreatedBy == filterParams.CreatedBy);

        return query.OrderByDescending(p => p.CreatedDate);
    }

    private static PromotionGetDto MapToGetDto(Promotion promotion)
    {
        return new PromotionGetDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            PercentDiscount = promotion.PercentDiscount,
            StartedDate = promotion.StartedDate,
            EndedDate = promotion.EndedDate,
            Status = promotion.Status,
            Description = promotion.Description,
            IsActive = promotion.IsActive,
            CreatedDate = promotion.CreatedDate,
            CreatedBy = promotion.CreatedBy,
            UpdatedDate = promotion.UpdatedDate,
            UpdatedBy = promotion.UpdatedBy
        };
    }
}
