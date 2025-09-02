namespace TheLightStore.Dtos.Products;

public class ProductAttributeDto
{
    public int Id { get; set; }
    public string AttributeName { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int DisplayOrder { get; set; }
}

