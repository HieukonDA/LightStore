using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Brand : BaseEntity<long>
{
    public string Code { get; set; } = null!;
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = [];
}
