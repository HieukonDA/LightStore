using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json.Serialization;

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

public partial class InventoryReservation
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public int? CartId { get; set; }

    public string? SessionId { get; set; }

    public int? UserId { get; set; }

    public int Quantity { get; set; }

    public DateTime ReservedUntil { get; set; }

    public string Status { get; set; } = null!;

    public int? OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ShoppingCart? Cart { get; set; }

    [JsonIgnore]
    public virtual Order? Order { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual GuestSession? Session { get; set; }

    public virtual User? User { get; set; }

    public virtual ProductVariants? Variant { get; set; }
}
