namespace TheLightStore.Models.Product;

public class ProductVariantAttribute
{
   public int Id { get; set; }
   public int VariantId { get; set; }
   public int AttributeId { get; set; }
   public int ValueId { get; set; } = 0;
   public string CustomValue { get; set; } = string.Empty;
}