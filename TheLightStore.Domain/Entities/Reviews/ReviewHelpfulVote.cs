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
namespace TheLightStore.Domain.Entities.Reviews;

public partial class ReviewHelpfulVote
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public string? IpAddress { get; set; }

    public bool IsHelpful { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ProductReview Review { get; set; } = null!;

    public virtual User? User { get; set; }
}
