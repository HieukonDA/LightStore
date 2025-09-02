namespace TheLightStore.Models.Products;


using TheLightStore.Models.Category;
using TheLightStore.Models.Repository;

public class Product
{
   public int Id { get; set; }
   public int CategoryId { get; set; }
   public int? BrandId { get; set; }
   public string Name { get; set; } = string.Empty;
   public string Slug { get; set; } = string.Empty;
   public string? ShortDescription { get; set; } = string.Empty;
   public string? Description { get; set; } = string.Empty;


   public string Sku { get; set; } = string.Empty;
   public decimal? BasePrice { get; set; } = 0;
   public decimal? SalePrice { get; set; } = 0;
   public decimal? Weight { get; set; } = 0;
   public string? Dimensions { get; set; } = string.Empty;

   public string? Origin { get; set; } = string.Empty;
   public string? WarrantyType { get; set; } = string.Empty;
   public int? WarrantyPeriod { get; set; } = 0;
   public bool ManageStock { get; set; } = true;
   public int StockQuantity { get; set; } = 0;
   public int StockAlertThreshold { get; set; } = 10;
   public bool AllowBackorder { get; set; } = false;
   public int VersionNumber { get; set; } = 1;
   public string? MetaTitle { get; set; } = string.Empty;
   public string? MetaDescription { get; set; } = string.Empty;
   public bool IsActive { get; set; } = true;
   public bool IsFeatured { get; set; } = false;
   public bool HasVariants { get; set; } = false;
   public DateTime CreatedAt { get; set; } = DateTime.Now;
   public DateTime? UpdatedAt { get; set; } = DateTime.Now;
   public virtual Category? Category { get; set; }
   public virtual Brand? Brand { get; set; }
}