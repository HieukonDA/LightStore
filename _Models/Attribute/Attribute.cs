namespace TheLightStore.Models.Attribute;

public class Attribute
{
   public int Id { get; set; }
   public string Name { get; set; } = string.Empty;
   public string DisplayName { get; set; } = string.Empty;
   public string Unit { get; set; } = string.Empty;
   public string InputType { get; set; } = string.Empty;
   public bool IsVariantAttribute { get; set; } = false;
   public bool IsFilterable { get; set; } = true;
   public int SortOrder { get; set; } = 0;
   public bool IsActive { get; set; } = true;
   public DateTime CreatedAt { get; set; } = DateTime.Now;
}
