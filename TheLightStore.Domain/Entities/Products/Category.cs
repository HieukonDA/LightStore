using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Category : BaseEntity<long>
{
    public string Code { get; set; } = null!;
    [Required]
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; } = [];
    public virtual ICollection<Product> Products { get; set; } = [];
}
