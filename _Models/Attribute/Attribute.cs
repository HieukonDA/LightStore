namespace TheLightStore.Models.Attributes;

public class Attribute
{
   public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Unit { get; set; }

    public string InputType { get; set; } = null!;

    public bool? IsVariantAttribute { get; set; }

    public bool? IsFilterable { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
