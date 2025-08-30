namespace TheLightStore.Models.Attribute;

public class AttributeValue
{
   public int Id { get; set; }
   public int AttributeId { get; set; }
   public string Value { get; set; } = string.Empty;
   public string DisplayValue { get; set; } = string.Empty;
   public string ColorCode { get; set; } = string.Empty;
   public int SortOrder { get; set; } = 0;
   public bool IsActive { get; set; } = true;
}