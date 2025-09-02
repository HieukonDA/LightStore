namespace TheLightStore.Dtos.Products;

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal SalePrice { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
    public List<ProductAttributeDto> Attributes { get; set; } = new();
}
