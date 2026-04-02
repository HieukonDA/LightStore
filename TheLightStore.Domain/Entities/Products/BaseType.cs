using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class BaseType : BaseEntity<long>
{
    public string Code { get; set; } = null!; // Vd: "BT-E27"
    [Required]
    public string Name { get; set; } = string.Empty; // Vd: "E27", "T8"
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = [];
}
