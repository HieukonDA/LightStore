using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class ColorTemperature : BaseEntity<long>
{
    public string Code { get; set; } = null!; // Vd: "CT-6000K"
    [Required]
    public string Name { get; set; } = string.Empty; // Vd: "Trắng 6000K", "Vàng 3000K"
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = [];
}
