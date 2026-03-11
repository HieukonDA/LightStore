using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
namespace TheLightStore.Domain.Entities.Shared;
public partial class InventoryLog
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int QuantityBefore { get; set; }

    public int QuantityChange { get; set; }

    public int QuantityAfter { get; set; }

    public string? Reason { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariants? Variant { get; set; }
}
