namespace TheLightStore.Interfaces.Products;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public string Sku { get; set; } = null!;
    
    // Pricing
    public decimal? BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public bool IsOnSale { get; set; }
    public decimal DiscountPercentage { get; set; }
    
    // Media
    public string? ThumbnailUrl { get; set; }
    
    // Stock & status
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewProduct { get; set; }
    
    // Analytics
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    
    // Related info (minimal)
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
}