using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TheLightStore.Application.DTOs.Products;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Entities;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Domain.Entities.Shared;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Infrastructure.Repositories.Products;

public class ProductVariantRepo : IProductVariantRepo
{
    private readonly DBContext _context;

    public ProductVariantRepo(DBContext context)
    {
        _context = context;
    }

    #region Basic CRUD

    public async Task<ProductVariants?> GetByIdAsync(int id, bool includeRelated = false)
    {
        var query = _context.ProductVariants.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(v => v.Product)
                .Include(v => v.ProductImages)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Attribute)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Value);
        }

        return await query.FirstOrDefaultAsync(v => v.Id == id && v.IsActive);
    }

    // 🔥 Admin method - lấy variant bất kể trạng thái (active/inactive)
    public async Task<ProductVariants?> GetByIdForAdminAsync(int id, bool includeRelated = false)
    {
        var query = _context.ProductVariants.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(v => v.Product)
                .Include(v => v.ProductImages)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Attribute)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Value);
        }

        return await query.FirstOrDefaultAsync(v => v.Id == id); // ✅ Không filter IsActive
    }

    public async Task<List<ProductVariants>> GetByProductIdAsync(int productId, bool includeRelated = false)
    {
        var query = _context.ProductVariants
            .Where(v => v.ProductId == productId && v.IsActive);

        if (includeRelated)
        {
            query = query
                .Include(v => v.Product)
                .Include(v => v.ProductImages)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Attribute)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Value);
        }

        return await query.OrderBy(v => v.SortOrder).ToListAsync();
    }

    // 🔥 Admin method - lấy tất cả variants theo productId (bao gồm inactive)
    public async Task<List<ProductVariants>> GetByProductIdForAdminAsync(int productId, bool includeRelated = false)
    {
        var query = _context.ProductVariants
            .Where(v => v.ProductId == productId); // ✅ Không filter IsActive

        if (includeRelated)
        {
            query = query
                .Include(v => v.Product)
                .Include(v => v.ProductImages)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Attribute)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Value);
        }

        return await query.OrderByDescending(v => v.IsActive).ThenBy(v => v.SortOrder).ToListAsync(); // ✅ Active trước, inactive sau
    }

    public async Task<ProductVariants?> GetBySkuAsync(string sku, bool includeRelated = false)
    {
        var query = _context.ProductVariants.AsQueryable();

        if (includeRelated)
        {
            query = query
                .Include(v => v.Product)
                .Include(v => v.ProductImages)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Attribute)
                .Include(v => v.ProductVariantAttributes)
                    .ThenInclude(a => a.Value);
        }

        return await query.FirstOrDefaultAsync(v => v.Sku == sku && v.IsActive);
    }

    public async Task<ProductVariants> AddAsync(ProductVariants productVariant)
    {
        await _context.ProductVariants.AddAsync(productVariant);
        await _context.SaveChangesAsync();
        return productVariant;
    }

    public async Task<ProductVariants> UpdateAsync(ProductVariants productVariant)
    {
        _context.ProductVariants.Update(productVariant);
        await _context.SaveChangesAsync();
        return productVariant;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var variant = await _context.ProductVariants.FindAsync(id);
        if (variant == null) return false;

        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProductVariants.AnyAsync(v => v.Id == id && v.IsActive);
    }

    // 🔥 Admin method - kiểm tra exist bất kể trạng thái
    public async Task<bool> ExistsForAdminAsync(int id)
    {
        return await _context.ProductVariants.AnyAsync(v => v.Id == id);
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        return await _context.ProductVariants.AnyAsync(v => 
            v.Sku == sku && 
            v.IsActive && 
            (excludeId == null || v.Id != excludeId));
    }

    // 🔥 Admin method - kiểm tra SKU exist bất kể trạng thái
    public async Task<bool> SkuExistsForAdminAsync(string sku, int? excludeId = null)
    {
        return await _context.ProductVariants.AnyAsync(v => 
            v.Sku == sku && 
            (excludeId == null || v.Id != excludeId));
    }

    #endregion

    #region Stock Management

    public async Task<bool> UpdateStockAsync(int variantId, int quantity)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null) return false;

        variant.StockQuantity = quantity;
        variant.UpdatedAt = DateTime.UtcNow;
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReserveStockAsync(int variantId, int quantity)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var variant = await _context.ProductVariants
                .Where(v => v.Id == variantId)
                .FirstOrDefaultAsync();

            if (variant == null || (variant.StockQuantity ?? 0) < quantity)
                return false;

            // Create inventory reservation record
            var reservation = new InventoryReservation
            {
                ProductId = variant.ProductId,
                VariantId = variantId,
                Quantity = quantity,
                Status = "Reserved",
                ReservedUntil = DateTime.UtcNow.AddMinutes(30), // 30 minutes expiry
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryReservations.Add(reservation);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ReleaseStockAsync(int variantId, int quantity)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove or reduce reservation
            var reservations = await _context.InventoryReservations
                .Where(r => r.VariantId == variantId && r.Status == "Reserved")
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            int remainingToRelease = quantity;
            foreach (var reservation in reservations)
            {
                if (remainingToRelease <= 0) break;

                if (reservation.Quantity <= remainingToRelease)
                {
                    remainingToRelease -= reservation.Quantity;
                    _context.InventoryReservations.Remove(reservation);
                }
                else
                {
                    reservation.Quantity -= remainingToRelease;
                    remainingToRelease = 0;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<Dictionary<int, ProductAvailabilityInfo>> GetVariantsAvailabilityWithLockAsync(
        List<int> variantIds,
        ITransaction transaction)
    {
        if (variantIds == null || variantIds.Count == 0)
            return new Dictionary<int, ProductAvailabilityInfo>();

        var sql = $@"
            SELECT Id, ProductId, StockQuantity
            FROM ProductVariants WITH (UPDLOCK, ROWLOCK)
            WHERE Id IN ({string.Join(",", variantIds)})
        ";

        var result = await _context.ProductVariants
            .FromSqlRaw(sql)
            .Select(v => new ProductAvailabilityInfo
            {
                ProductId = v.ProductId,
                VariantId = v.Id,
                AvailableQuantity = (int)(v.StockQuantity ?? 0)
            })
            .ToDictionaryAsync(x => x.VariantId!.Value);

        return result;
    }

    #endregion

    #region Attribute Management

    public async Task<List<ProductVariantAttribute>> GetVariantAttributesAsync(int variantId)
    {
        return await _context.ProductVariantAttributes
            .Include(a => a.Attribute)
            .Include(a => a.Value)
            .Where(a => a.VariantId == variantId)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();
    }

    public async Task<bool> UpdateVariantAttributesAsync(int variantId, List<ProductVariantAttribute> attributes)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove existing attributes
            var existingAttributes = await _context.ProductVariantAttributes
                .Where(a => a.VariantId == variantId)
                .ToListAsync();

            _context.ProductVariantAttributes.RemoveRange(existingAttributes);

            // Add new attributes
            foreach (var attribute in attributes)
            {
                attribute.VariantId = variantId;
                attribute.CreatedAt = DateTime.UtcNow;
                attribute.UpdatedAt = DateTime.UtcNow;
                _context.ProductVariantAttributes.Add(attribute);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    #endregion

    #region Analytics & Filtering

    public async Task<List<ProductVariants>> GetLowStockVariantsAsync(int? productId = null)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive && 
                       v.StockQuantity.HasValue && 
                       v.StockAlertThreshold.HasValue &&
                       v.StockQuantity <= v.StockAlertThreshold);

        if (productId.HasValue)
        {
            query = query.Where(v => v.ProductId == productId.Value);
        }

        return await query.OrderBy(v => v.StockQuantity).ToListAsync();
    }

    public async Task<List<ProductVariants>> GetOutOfStockVariantsAsync(int? productId = null)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive && (!v.StockQuantity.HasValue || v.StockQuantity <= 0));

        if (productId.HasValue)
        {
            query = query.Where(v => v.ProductId == productId.Value);
        }

        return await query.OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<List<ProductVariants>> GetActiveVariantsAsync(int? productId = null)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive);

        if (productId.HasValue)
        {
            query = query.Where(v => v.ProductId == productId.Value);
        }

        return await query.OrderBy(v => v.SortOrder).ThenBy(v => v.Name).ToListAsync();
    }

    #endregion

    #region Sorting

    public async Task<bool> UpdateSortOrderAsync(int variantId, int sortOrder)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null) return false;

        variant.SortOrder = sortOrder;
        variant.UpdatedAt = DateTime.UtcNow;
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProductVariants>> GetVariantsByProductOrderedAsync(int productId)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.ProductImages)
            .Where(v => v.ProductId == productId && v.IsActive)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.Name)
            .ToListAsync();
    }

    #endregion
}
