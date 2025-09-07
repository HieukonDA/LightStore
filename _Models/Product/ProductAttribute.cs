namespace TheLightStore.Models.Products;
using TheLightStore.Models.Attributes;

public class ProductAttribute
{
   public int Id { get; set; }

    public int ProductId { get; set; }

    public int AttributeId { get; set; }

    public int? ValueId { get; set; }

    public string? CustomValue { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual AttributeValue? Value { get; set; }
}