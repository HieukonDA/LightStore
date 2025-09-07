namespace TheLightStore.Models.Products;

public class Product
{
   public int Id { get; set; }

    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string Sku { get; set; } = null!;

    public decimal? BasePrice { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? Weight { get; set; }

    public string? Dimensions { get; set; }

    public string? Origin { get; set; }

    public string? WarrantyType { get; set; }

    public int? WarrantyPeriod { get; set; }

    public bool ManageStock { get; set; }

    public int StockQuantity { get; set; }

    public int StockAlertThreshold { get; set; }

    public bool AllowBackorder { get; set; }

    public int VersionNumber { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public bool HasVariants { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}