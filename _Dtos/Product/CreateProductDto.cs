namespace TheLightStore.Dtos.Products;

public class CreateProductDto
{
    // Basic Information (Required)
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; }
    public string Sku { get; set; } = null!;
    public decimal BasePrice { get; set; }
    
    // Optional Information
    public int? BrandId { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal SalePrice { get; set; }
    
    // Product Details
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Origin { get; set; }
    public string? WarrantyType { get; set; }
    public int? WarrantyPeriod { get; set; }
    
    // Stock Management
    public bool ManageStock { get; set; } = true;
    public int StockQuantity { get; set; } = 0;
    public int StockAlertThreshold { get; set; } = 10;
    public bool AllowBackorder { get; set; } = false;
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    
    // Status Flags
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool HasVariants { get; set; } = false;
    
    // Related Data
    public List<CreateProductImageDto>? Images { get; set; }
    public List<CreateProductVariantDto>? Variants { get; set; }
}

