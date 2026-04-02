using System;
using System.ComponentModel.DataAnnotations;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class Promotion : BaseEntity<long>
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public long PercentDiscount { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime EndedDate { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = [];
}
