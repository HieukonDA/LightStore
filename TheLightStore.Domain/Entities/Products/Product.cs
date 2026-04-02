using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Product : BaseEntity<long>
{
    public string Code { get; set; } = null!;
    public string ProductType { get; set; } = "SELF_PRODUCED";
    public long CategoryId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public bool IsInBusiness { get; set; }
    public bool IsOrderedOnline { get; set; }
    public bool IsPackaged { get; set; }
    public string? Position { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long? BrandId { get; set; }
    public double? AverageRatingPoint { get; set; }
    public long? TotalSoldQuantity { get; set; }

    // Navigation properties
    public virtual Brand? Brand { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = [];
    public virtual ICollection<ProductImage> ProductImages { get; set; } = [];
    public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = [];
}
