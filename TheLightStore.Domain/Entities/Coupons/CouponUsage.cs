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
namespace TheLightStore.Domain.Entities.Coupons;

public partial class CouponUsage
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual User? User { get; set; }
}
