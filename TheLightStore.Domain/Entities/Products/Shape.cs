using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Shape : BaseEntity<long>
{
    public string Code { get; set; } = null!; // Vd: "SH-ROUND"
    [Required]
    public string Name { get; set; } = string.Empty; // Vd: "Tròn", "Vuông"
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = [];
}
