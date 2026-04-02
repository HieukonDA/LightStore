using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Power : BaseEntity<long>
{
    public string Code { get; set; } = null!; // Vd: "PW-9W"
    [Required]
    public string Name { get; set; } = string.Empty; // Vd: "9W", "12W"
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = [];
}
