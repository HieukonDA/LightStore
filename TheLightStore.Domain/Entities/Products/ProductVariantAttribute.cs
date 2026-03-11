using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheLightStore.Domain.Entities.Shared;

using TheLightStore.Domain.Entities.Products;
using TheLightStore.Domain.Entities.Orders;
using TheLightStore.Domain.Entities.Auth;
using TheLightStore.Domain.Entities.Blogs;
using TheLightStore.Domain.Entities.Carts;
using TheLightStore.Domain.Entities.Coupons;
using TheLightStore.Domain.Entities.Notifications;
using TheLightStore.Domain.Entities.Reviews;
using TheLightStore.Domain.Entities.Shipping;
using TheLightStore.Domain.Entities.Shared;
namespace TheLightStore.Domain.Entities.Products;

public class ProductVariantAttribute
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    public int AttributeId { get; set; }
    public int? ValueId { get; set; }
    public string? CustomValue { get; set; }
    public int SortOrder { get; set; } = 0;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ProductVariants Variant { get; set; } = null!;
    public virtual TheLightStore.Domain.Entities.Shared.Attribute Attribute { get; set; } = null!;
    public virtual AttributeValue? Value { get; set; }
}