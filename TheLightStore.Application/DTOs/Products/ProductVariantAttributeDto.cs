namespace TheLightStore.Application.DTOs.Products;

public class ProductVariantAttributeDto
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = null!;
    public string AttributeDisplayName { get; set; } = null!;
    public int ValueId { get; set; }
    public string Value { get; set; } = null!;
    public string? DisplayValue { get; set; }
    public string? CustomValue { get; set; }
    public string? ColorCode { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
}

public class StockStatusDto
{
    public int VariantId { get; set; }
    public int StockQuantity { get; set; }
    public int StockAlertThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsInStock { get; set; }
    public int? ReservedQuantity { get; set; }
    public int? AvailableQuantity { get; set; }
}