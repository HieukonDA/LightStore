
namespace TheLightStore.Dtos.Cart;

public class SavedCartDto
{
    public int Id { get; set; }
    public string CartName { get; set; }
    public int ItemsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Preview items (first 3-5 items)
    public List<SavedCartItemDto> PreviewItems { get; set; }
}

public class SavedCartItemDto  
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtSave { get; set; }
}