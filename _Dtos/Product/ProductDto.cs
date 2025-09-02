namespace TheLightStore.Dtos.Products;

public class ProductDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Sku { get; set; } = null!;
    
    // Pricing
    public decimal? BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public bool IsOnSale { get; set; }
    public decimal DiscountPercentage { get; set; }
    
    // Physical attributes
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    
    // Product info
    public string? Origin { get; set; }
    public string? WarrantyType { get; set; }
    public int? WarrantyPeriod { get; set; }
    
    // Stock management
    public bool ManageStock { get; set; }
    public int StockQuantity { get; set; }
    public int StockAlertThreshold { get; set; }
    public bool AllowBackorder { get; set; }
    public bool IsInStock { get; set; }
    
    // Version control
    public int VersionNumber { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    
    // Status flags
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool HasVariants { get; set; }
    public bool IsNewProduct { get; set; }
    
    // Media
    public string? ThumbnailUrl { get; set; }
    public List<ProductImageDto>? Images { get; set; }
    
    // Analytics & ratings
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    
    // Related entities
    public CategoryDto Category { get; set; } = null!;
    public BrandDto? Brand { get; set; }
    public List<ProductVariantDto>? Variants { get; set; }
    public List<ProductAttributeDto>? Attributes { get; set; }
    public ProductSpecsDto? Specifications { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
