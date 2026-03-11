namespace TheLightStore.Application.DTOs.Products;

public class CreateProductVariantDto
{
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public int? StockAlertThreshold { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsActive { get; set; } = true;
    public int? SortOrder { get; set; }
    public List<ProductVariantAttributeDto>? Attributes { get; set; }
}
