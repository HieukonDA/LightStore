namespace TheLightStore.Repositories.Products;

public class ProductVariantRepo : IProductVariantRepo
{
    private readonly DBContext _context;

    public ProductVariantRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<ProductVariant?> GetByIdAsync(int id)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == id && v.IsActive);
    }

    public async Task<ProductVariant?> GetByProductIdAsync(int productId)
    {
        return await _context.ProductVariants
            .Where(v => v.ProductId == productId && v.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<ProductVariant?> GetBySkuAsync(string sku)
    {
        return await _context.ProductVariants
            .Where(v => v.Sku == sku && v.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<ProductVariant> AddAsync(ProductVariant productVariant)
    {
        await _context.ProductVariants.AddAsync(productVariant);
        await _context.SaveChangesAsync();
        return productVariant;
    }

    public async Task<ProductVariant> UpdateAsync(ProductVariant productVariant)
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

    public async Task<bool> UpdateStockAsync(int variantId, int quantity)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null) return false;

        variant.StockQuantity = quantity;
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
        return true;
    }
}