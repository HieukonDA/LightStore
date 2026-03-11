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
namespace TheLightStore.Domain.Entities.Products;
using TheLightStore.Domain.Entities.Shared;

public class ProductAttribute
{
   public int Id { get; set; }

    public int ProductId { get; set; }

    public int AttributeId { get; set; }

    public int? ValueId { get; set; }

    public string? CustomValue { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual AttributeValue? Value { get; set; }
}