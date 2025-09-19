namespace TheLightStore.Dtos.Orders;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }

    public string ProductName { get; set; } = null!;
    public string ProductSku { get; set; } = null!;
    public string? VariantName { get; set; }

    public string? ProductAttributes { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
