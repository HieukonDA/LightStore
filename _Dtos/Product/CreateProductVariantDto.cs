namespace TheLightStore.Dtos.Products;

public class CreateProductVariantDto
{
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int StockQuantity { get; set; } = 0;
}
