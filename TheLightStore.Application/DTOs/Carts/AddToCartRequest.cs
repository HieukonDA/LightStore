namespace TheLightStore.Application.DTOs.Carts;

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
}