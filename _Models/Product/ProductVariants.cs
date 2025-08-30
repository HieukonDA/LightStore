namespace TheLightStore.Models.Product;

public class ProductVariant
{
   public int Id { get; set; }
   public int ProductId { get; set; }
   public string Sku { get; set; } = string.Empty;
   public string Name { get; set; } = string.Empty;
   public decimal Price { get; set; } = 0;
   public decimal SalePrice { get; set; } = 0;
   public decimal CostPrice { get; set; } = 0;
   public int StockQuantity { get; set; } = 0;
   public int StockAlertThreshold { get; set; } = 0;
   public int VersionNumber { get; set; } = 1;
   public decimal Weight { get; set; } = 0;
   public string Dimensions { get; set; } = string.Empty;
   public bool IsActive { get; set; } = true;
   public int SortOrder { get; set; } = 0;
   public DateTime CreatedAt { get; set; } = DateTime.Now;
   public DateTime UpdatedAt { get; set; } = DateTime.Now;
}