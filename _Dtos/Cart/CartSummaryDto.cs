namespace TheLightStore.Dtos.Cart;
public class CartSummaryDto
{
    public int CartId { get; set; }
    public int ItemsCount { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime? LastUpdated { get; set; }
}