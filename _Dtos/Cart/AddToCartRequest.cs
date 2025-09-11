namespace TheLightStore.Dtos.Cart;

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
}