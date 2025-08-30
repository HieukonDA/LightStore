namespace TheLightStore.Models.Product;

public class ProductImage
{
   public int Id { get; set; }
   public int ProductId { get; set; }
   public int VariantId { get; set; } = 0;
   public string ImageUrl { get; set; } = string.Empty;
   public string AltText { get; set; } = string.Empty;
   public bool IsPrimary { get; set; } = false;
   public int SortOrder { get; set; } = 0;
   public DateTime CreatedAt { get; set; } = DateTime.Now;
}