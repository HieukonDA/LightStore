namespace TheLightStore.Application.DTOs.Cart;

public class UpdateQuantityRequest
{
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }
    public int? VariantId { get; set; }
}