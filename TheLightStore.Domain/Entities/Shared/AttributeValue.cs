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

public class AttributeValue
{
   public int Id { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public string DisplayValue { get; set; } = null!;

    public string? ColorCode { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}