public class UpdateProductDto
{
    // Basic Information
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string? Sku { get; set; }
    public decimal BasePrice { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal? SalePrice { get; set; }
    
    // Product Details
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Origin { get; set; }
    public string? WarrantyType { get; set; }
    public int? WarrantyPeriod { get; set; }
    
    // Stock Management
    public bool? ManageStock { get; set; }
    public int? StockQuantity { get; set; }
    public int? StockAlertThreshold { get; set; }
    public bool? AllowBackorder { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    
    // Status Flags
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? HasVariants { get; set; }
    
    // Related Data
    public List<UpdateProductImageDto>? Images { get; set; }
}
