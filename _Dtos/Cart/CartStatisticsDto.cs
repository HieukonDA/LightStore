namespace TheLightStore.Dtos.Cart;

public class CartStatisticsDto
{
    public int TotalActiveCarts { get; set; }
    public int TotalAbandonedCarts { get; set; }
    public decimal TotalCartsValue { get; set; }
    public decimal AverageCartValue { get; set; }
    public int TotalCartItems { get; set; }
    public DateTime GeneratedAt { get; set; }
}