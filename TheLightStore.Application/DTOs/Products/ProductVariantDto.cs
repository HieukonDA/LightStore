namespace TheLightStore.Application.DTOs.Products;

public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
    public int StockAlertThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProductVariantAttributeDto> Attributes { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
}
