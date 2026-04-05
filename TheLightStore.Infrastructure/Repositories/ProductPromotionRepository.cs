using System;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.ProductPromotionDto;

namespace TheLightStore.Infrastructure.Repositories;

public class ProductPromotionRepository : IProductPromotionRepository
{
    private readonly DBContext _context;

    public ProductPromotionRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<ProductPromotionGetDto>> GetAllAsync(ProductPromotionFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Include(pp => pp.Product)
            .Include(pp => pp.Promotion)
            .Select(pp => MapToGetDto(pp))
            .ToListAsync();

        return new PaginationModel<ProductPromotionGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<ProductPromotionGetDto?> GetByIdAsync(long id)
    {
        var productPromotion = await _context.ProductPromotions
            .Include(pp => pp.Product)
            .Include(pp => pp.Promotion)
            .FirstOrDefaultAsync(pp => pp.Id == id);

        return productPromotion == null ? null : MapToGetDto(productPromotion);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.ProductPromotions.AnyAsync(pp => pp.Id == id);
    }

    public async Task<long> CreateAsync(ProductPromotion productPromotion)
    {
        _context.ProductPromotions.Add(productPromotion);
        await _context.SaveChangesAsync();
        return productPromotion.Id;
    }

    public async Task<bool> UpdateAsync(ProductPromotion productPromotion)
    {
        _context.ProductPromotions.Update(productPromotion);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var productPromotion = await _context.ProductPromotions.FindAsync(id);
        if (productPromotion == null)
            return false;

        _context.ProductPromotions.Remove(productPromotion);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<ProductPromotionGetDto>> GetAllActiveAsync()
    {
        return await _context.ProductPromotions
            .Where(pp => pp.IsActive)
            .Include(pp => pp.Product)
            .Include(pp => pp.Promotion)
            .OrderByDescending(pp => pp.CreatedDate)
            .Select(pp => MapToGetDto(pp))
            .ToListAsync();
    }

    public async Task<List<ProductsByPromotionDto>> GetProductsByPromotionIdAsync(long promotionId)
    {
        return await _context.ProductPromotions
            .Where(pp => pp.PromotionId == promotionId && pp.IsActive)
            .Include(pp => pp.Product)
            .ThenInclude(p => p!.Category)
            .Include(pp => pp.Product)
            .ThenInclude(p => p!.Brand)
            .Select(pp => new ProductsByPromotionDto
            {
                Id = pp.Product!.Id,
                Name = pp.Product.Name,
                Code = pp.Product.Code,
                Description = pp.Product.Description,
                CategoryId = pp.Product.CategoryId,
                CategoryName = pp.Product.Category!.Name,
                BrandId = pp.Product.BrandId,
                BrandName = pp.Product.Brand!.Name,
                IsActive = pp.Product.IsActive
            })
            .Distinct()
            .ToListAsync();
    }

    private IQueryable<ProductPromotion> BuildQueryable(ProductPromotionFilterParams filterParams)
    {
        var query = _context.ProductPromotions.AsQueryable();

        if (filterParams.ProductId.HasValue)
            query = query.Where(pp => pp.ProductId == filterParams.ProductId.Value);

        if (filterParams.PromotionId.HasValue)
            query = query.Where(pp => pp.PromotionId == filterParams.PromotionId.Value);

        if (filterParams.IsActive.HasValue)
            query = query.Where(pp => pp.IsActive == filterParams.IsActive.Value);

        if (!string.IsNullOrEmpty(filterParams.CreatedBy))
            query = query.Where(pp => pp.CreatedBy == filterParams.CreatedBy);

        return query.OrderByDescending(pp => pp.CreatedDate);
    }

    private static ProductPromotionGetDto MapToGetDto(ProductPromotion productPromotion)
    {
        return new ProductPromotionGetDto
        {
            Id = productPromotion.Id,
            ProductId = productPromotion.ProductId,
            PromotionId = productPromotion.PromotionId,
            IsActive = productPromotion.IsActive,
            CreatedDate = productPromotion.CreatedDate,
            CreatedBy = productPromotion.CreatedBy,
            UpdatedDate = productPromotion.UpdatedDate,
            UpdatedBy = productPromotion.UpdatedBy,
            Product = productPromotion.Product == null ? null : new ProductDto.ProductGetDto
            {
                Id = productPromotion.Product.Id,
                Name = productPromotion.Product.Name,
                Code = productPromotion.Product.Code
            },
            Promotion = productPromotion.Promotion == null ? null : new PromotionDto.PromotionGetDto
            {
                Id = productPromotion.Promotion.Id,
                Name = productPromotion.Promotion.Name,
                Code = productPromotion.Promotion.Code,
                PercentDiscount = productPromotion.Promotion.PercentDiscount
            }
        };
    }
}
