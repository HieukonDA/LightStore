namespace TheLightStore.Models.Product;

public class ProductAttribute
{
   public int Id { get; set; }
   public int ProductId { get; set; }
   public int AttributeId { get; set; }
   public int ValueId { get; set; } = 0;
   public string CustomValue { get; set; } = string.Empty;
   public int DisplayOrder { get; set; } = 0;
}