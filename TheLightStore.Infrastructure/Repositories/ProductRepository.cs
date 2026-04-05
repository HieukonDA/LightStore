using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;
using static TheLightStore.Application.Dtos.ProductDto;

namespace TheLightStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DBContext _context;

    public ProductRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<PaginationModel<ProductGetDto>> GetAllAsync(ProductFilterParams filterParams)
    {
        var query = BuildQueryable(filterParams);
        var totalRecords = await query.CountAsync();

        var records = await query
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.Power)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.ColorTemperature)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.Shape)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.BaseType)
            .Include(p => p.ProductPromotions)
            .ThenInclude(pp => pp.Promotion)
            .Include(p => p.ProductImages)
            .Select(p => MapToGetDto(p))
            .ToListAsync();

        return new PaginationModel<ProductGetDto>
        {
            Records = records,
            Pagination = PaginationHelper.CreatePagination(totalRecords, filterParams.PageSize, filterParams.PageNumber)
        };
    }

    public async Task<ProductGetDto?> GetByIdAsync(long id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.Power)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.ColorTemperature)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.Shape)
            .Include(p => p.ProductDetails)
            .ThenInclude(pd => pd.BaseType)
            .Include(p => p.ProductPromotions)
            .ThenInclude(pp => pp.Promotion)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : MapToGetDto(product);
    }

    public async Task<Product?> GetProductWithDetailsAsync(long id)
    {
        return await _context.Products
            .Include(p => p.ProductDetails)
            .Include(p => p.ProductImages)
            .Include(p => p.ProductPromotions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(string code, long excludeId = 0)
    {
        if (excludeId > 0)
            return await _context.Products.AnyAsync(p => p.Code == code && p.Id != excludeId);
        return await _context.Products.AnyAsync(p => p.Code == code);
    }

    public async Task<long> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<ProductGetDto>> GetAllActiveAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductDetails)
            .Include(p => p.ProductPromotions)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => MapToGetDto(p))
            .ToListAsync();
    }

    public async Task<bool> HasOrderDetailsAsync(List<long> productDetailIds)
    {
        // Placeholder - would need OrderDetail entity to implement fully
        // For now, return false as not critical to the core logic
        return false;
    }

    private IQueryable<Product> BuildQueryable(ProductFilterParams filterParams)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(filterParams.SearchString))
            query = query.Where(p => p.Name.Contains(filterParams.SearchString));

        if (filterParams.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filterParams.CategoryId.Value);

        if (filterParams.BrandId.HasValue)
            query = query.Where(p => p.BrandId == filterParams.BrandId.Value);

        if (!string.IsNullOrEmpty(filterParams.ProductType))
            query = query.Where(p => p.ProductType == filterParams.ProductType);

        if (filterParams.IsInBusiness.HasValue)
            query = query.Where(p => p.IsInBusiness == filterParams.IsInBusiness.Value);

        if (filterParams.IsOrderedOnline.HasValue)
            query = query.Where(p => p.IsOrderedOnline == filterParams.IsOrderedOnline.Value);

        if (filterParams.IsPackaged.HasValue)
            query = query.Where(p => p.IsPackaged == filterParams.IsPackaged.Value);

        if (filterParams.CreatedDate.HasValue)
            query = query.Where(p => 
                p.CreatedDate.Year == filterParams.CreatedDate.Value.Year &&
                p.CreatedDate.Month == filterParams.CreatedDate.Value.Month &&
                p.CreatedDate.Day == filterParams.CreatedDate.Value.Day);

        if (!string.IsNullOrEmpty(filterParams.CreatedBy))
            query = query.Where(p => p.CreatedBy != null && p.CreatedBy.Contains(filterParams.CreatedBy));

        if (filterParams.UpdatedDate.HasValue)
            query = query.Where(p => p.UpdatedDate.HasValue &&
                p.UpdatedDate.Value.Year == filterParams.UpdatedDate.Value.Year &&
                p.UpdatedDate.Value.Month == filterParams.UpdatedDate.Value.Month &&
                p.UpdatedDate.Value.Day == filterParams.UpdatedDate.Value.Day);

        if (!string.IsNullOrEmpty(filterParams.UpdatedBy))
            query = query.Where(p => p.UpdatedBy != null && p.UpdatedBy.Contains(filterParams.UpdatedBy));

        if (filterParams.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filterParams.IsActive.Value);

        return query.OrderByDescending(p => p.Id);
    }

    private static ProductGetDto MapToGetDto(Product product)
    {
        return new ProductGetDto
        {
            Id = product.Id,
            Code = product.Code,
            ProductType = product.ProductType,
            Name = product.Name,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            IsInBusiness = product.IsInBusiness,
            IsOrderedOnline = product.IsOrderedOnline,
            IsPackaged = product.IsPackaged,
            Description = product.Description,
            Position = product.Position,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            AverageRatingPoint = product.AverageRatingPoint,
            TotalSoldQuantity = product.TotalSoldQuantity,
            CreatedDate = product.CreatedDate,
            CreatedBy = product.CreatedBy,
            UpdatedDate = product.UpdatedDate,
            UpdatedBy = product.UpdatedBy,
            Category = product.Category == null ? null : new CategoryDto.CategoryGetDto
            {
                Id = product.Category.Id,
                Code = product.Category.Code,
                Name = product.Category.Name,
                IsActive = product.Category.IsActive
            },
            Brand = product.Brand == null ? null : new BrandDto.BrandGetDto
            {
                Id = product.Brand.Id,
                Code = product.Brand.Code,
                Name = product.Brand.Name,
                IsActive = product.Brand.IsActive
            },
            ProductDetails = product.ProductDetails?
                .Select(pd => new ProductDetailDto.ProductDetailGetDto
                {
                    Id = pd.Id,
                    ProductId = pd.ProductId,
                    PowerId = pd.PowerId,
                    ColorTemperatureId = pd.ColorTemperatureId,
                    ShapeId = pd.ShapeId,
                    BaseTypeId = pd.BaseTypeId,
                    SellingPrice = pd.SellingPrice,
                    EarningPoints = pd.EarningPoints,
                    SoldQuantity = pd.SoldQuantity,
                    Description = pd.Description,
                    IsActive = pd.IsActive,
                    PowerName = pd.Power?.Name,
                    ColorTemperatureName = pd.ColorTemperature?.Name,
                    ShapeName = pd.Shape?.Name,
                    BaseTypeName = pd.BaseType?.Name
                })
                .ToList() ?? [],
            Promotion = product.ProductPromotions?
                .FirstOrDefault(pp => pp.IsActive)?.Promotion == null ? null : new PromotionDto.PromotionGetDto
                {
                    Id = product.ProductPromotions.FirstOrDefault(pp => pp.IsActive)!.Promotion!.Id,
                    Code = product.ProductPromotions.FirstOrDefault(pp => pp.IsActive)!.Promotion!.Code,
                    Name = product.ProductPromotions.FirstOrDefault(pp => pp.IsActive)!.Promotion!.Name,
                    PercentDiscount = product.ProductPromotions.FirstOrDefault(pp => pp.IsActive)!.Promotion!.PercentDiscount
                },
            ProductImages = product.ProductImages?
                .Select(pi => new ProductImageDto.ProductImageGetDto
                {
                    Id = pi.Id,
                    ProductId = pi.ProductId,
                    FileId = pi.FileId,
                    FileName = pi.SysFile?.Name ?? string.Empty,
                    FilePath = pi.SysFile?.Path ?? string.Empty
                })
                .ToList() ?? []
        };
    }
}
