namespace TheLightStore.DTOs.Orders;

public class OrderItemCreateDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Giá tại thời điểm đặt
    public string ProductName { get; set; } = null!;
    public string ProductSku { get; set; } = null!;
    public string? VariantName { get; set; }
    public string? ProductAttributes { get; set; }
}
