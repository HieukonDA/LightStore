namespace TheLightStore.Models.Attributes;

public class AttributeValue
{
   public int Id { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayValue { get; set; } = null!;

    public string? ColorCode { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}