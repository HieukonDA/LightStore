namespace TheLightStore.Dtos.Cart;
public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductSku { get; set; }
    public string? ProductImageUrl { get; set; }
    
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public bool IsAvailable { get; set; } // Product still active?
    public DateTime AddedAt { get; set; }
}