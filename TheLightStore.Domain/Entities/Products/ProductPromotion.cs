using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class ProductPromotion : BaseEntity<long>
{
    public long ProductId { get; set; }
    public long PromotionId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Promotion Promotion { get; set; } = null!;
}
