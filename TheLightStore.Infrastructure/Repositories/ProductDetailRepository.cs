using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.ProductDetailDto;

namespace TheLightStore.Infrastructure.Repositories;

public class ProductDetailRepository : IProductDetailRepository
{
    private readonly DBContext _context;

    public ProductDetailRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<ProductDetailGetDto>> GetAllAsync(ProductDetailFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Include(pd => pd.Product)
            .Include(pd => pd.Power)
            .Include(pd => pd.ColorTemperature)
            .Include(pd => pd.Shape)
            .Include(pd => pd.BaseType)
            .Select(pd => MapToGetDto(pd))
            .ToListAsync();

        return new PaginationModel<ProductDetailGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<ProductDetailGetDto?> GetByIdAsync(long id)
    {
        var productDetail = await _context.ProductDetails
            .Include(pd => pd.Product)
            .Include(pd => pd.Power)
            .Include(pd => pd.ColorTemperature)
            .Include(pd => pd.Shape)
            .Include(pd => pd.BaseType)
            .FirstOrDefaultAsync(pd => pd.Id == id);

        return productDetail == null ? null : MapToGetDto(productDetail);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.ProductDetails.AnyAsync(pd => pd.Id == id);
    }

    public async Task<long> CreateAsync(ProductDetail productDetail)
    {
        _context.ProductDetails.Add(productDetail);
        await _context.SaveChangesAsync();
        return productDetail.Id;
    }

    public async Task<bool> UpdateAsync(ProductDetail productDetail)
    {
        _context.ProductDetails.Update(productDetail);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var productDetail = await _context.ProductDetails.FindAsync(id);
        if (productDetail == null)
            return false;

        _context.ProductDetails.Remove(productDetail);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<ProductDetailGetDto>> GetAllActiveAsync()
    {
        return await _context.ProductDetails
            .Where(pd => pd.IsActive)
            .Include(pd => pd.Product)
            .Include(pd => pd.Power)
            .Include(pd => pd.ColorTemperature)
            .Include(pd => pd.Shape)
            .Include(pd => pd.BaseType)
            .OrderByDescending(pd => pd.CreatedDate)
            .Select(pd => MapToGetDto(pd))
            .ToListAsync();
    }

    public async Task<List<ProductDetail>> GetProductDetailsByProductIdAsync(long productId)
    {
        return await _context.ProductDetails
            .Where(pd => pd.ProductId == productId)
            .Include(pd => pd.Power)
            .Include(pd => pd.ColorTemperature)
            .Include(pd => pd.Shape)
            .Include(pd => pd.BaseType)
            .ToListAsync();
    }

    private IQueryable<ProductDetail> BuildQueryable(ProductDetailFilterParams filterParams)
    {
        var query = _context.ProductDetails.AsQueryable();

        if (filterParams.ProductId.HasValue)
            query = query.Where(pd => pd.ProductId == filterParams.ProductId.Value);

        if (filterParams.PowerId.HasValue)
            query = query.Where(pd => pd.PowerId == filterParams.PowerId.Value);

        if (filterParams.ColorTemperatureId.HasValue)
            query = query.Where(pd => pd.ColorTemperatureId == filterParams.ColorTemperatureId.Value);

        if (filterParams.ShapeId.HasValue)
            query = query.Where(pd => pd.ShapeId == filterParams.ShapeId.Value);

        if (filterParams.BaseTypeId.HasValue)
            query = query.Where(pd => pd.BaseTypeId == filterParams.BaseTypeId.Value);

        if (filterParams.CreatedDate.HasValue)
            query = query.Where(pd => 
                pd.CreatedDate.Year == filterParams.CreatedDate.Value.Year &&
                pd.CreatedDate.Month == filterParams.CreatedDate.Value.Month &&
                pd.CreatedDate.Day == filterParams.CreatedDate.Value.Day);

        if (!string.IsNullOrEmpty(filterParams.CreatedBy))
            query = query.Where(pd => pd.CreatedBy != null && pd.CreatedBy.Contains(filterParams.CreatedBy));

        if (filterParams.UpdatedDate.HasValue)
            query = query.Where(pd => pd.UpdatedDate.HasValue &&
                pd.UpdatedDate.Value.Year == filterParams.UpdatedDate.Value.Year &&
                pd.UpdatedDate.Value.Month == filterParams.UpdatedDate.Value.Month &&
                pd.UpdatedDate.Value.Day == filterParams.UpdatedDate.Value.Day);

        if (!string.IsNullOrEmpty(filterParams.UpdatedBy))
            query = query.Where(pd => pd.UpdatedBy != null && pd.UpdatedBy.Contains(filterParams.UpdatedBy));

        if (filterParams.IsActive.HasValue)
            query = query.Where(pd => pd.IsActive == filterParams.IsActive.Value);

        return query.OrderByDescending(pd => pd.Id);
    }

    private static ProductDetailGetDto MapToGetDto(ProductDetail productDetail)
    {
        return new ProductDetailGetDto
        {
            Id = productDetail.Id,
            ProductId = productDetail.ProductId,
            PowerId = productDetail.PowerId,
            ColorTemperatureId = productDetail.ColorTemperatureId,
            ShapeId = productDetail.ShapeId,
            BaseTypeId = productDetail.BaseTypeId,
            CurrencyId = productDetail.CurrencyId,
            SellingPrice = productDetail.SellingPrice,
            EarningPoints = productDetail.EarningPoints,
            SoldQuantity = productDetail.SoldQuantity,
            Description = productDetail.Description,
            IsActive = productDetail.IsActive,
            CreatedDate = productDetail.CreatedDate,
            CreatedBy = productDetail.CreatedBy,
            UpdatedDate = productDetail.UpdatedDate,
            UpdatedBy = productDetail.UpdatedBy,
            ProductName = productDetail.Product?.Name,
            PowerName = productDetail.Power?.Name,
            ColorTemperatureName = productDetail.ColorTemperature?.Name,
            ShapeName = productDetail.Shape?.Name,
            BaseTypeName = productDetail.BaseType?.Name
        };
    }
}
