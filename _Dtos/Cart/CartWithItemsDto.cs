namespace TheLightStore.Dtos.Cart;
public class CartWithItemsDto
{
    public int Id { get; set; }
    public int ItemsCount { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime? LastUpdated { get; set; }
    
    public List<CartItemDto> Items { get; set; }
}