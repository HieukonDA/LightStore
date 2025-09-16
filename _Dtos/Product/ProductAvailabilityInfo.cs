namespace TheLightStore.Dtos.Product;
public class ProductAvailabilityInfo
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }   // null nếu product không có variant
    public int AvailableQuantity { get; set; }
}
