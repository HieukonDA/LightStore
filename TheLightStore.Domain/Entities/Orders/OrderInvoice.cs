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
namespace TheLightStore.Domain.Entities.Orders;

public partial class OrderInvoice
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public bool? InvoiceRequired { get; set; }

    public string? InvoiceType { get; set; }

    public string? IndividualName { get; set; }

    public string? CompanyName { get; set; }

    public string? TaxCode { get; set; }

    public string? CompanyAddress { get; set; }

    public string? InvoiceFileUrl { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
