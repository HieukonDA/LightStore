using System;
using System.Text.Json.Serialization;
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

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? ShippingCost { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string? CustomerNotes { get; set; }

    public string? AdminNotes { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int? VersionNumber { get; set; }

    // GHN Integration fields
    public string? GHNOrderCode { get; set; } // Mã don hàng GHN
    public string? GHNSortCode { get; set; } // Mã phân lo?i GHN
    public string? GHNTransType { get; set; } // Lo?i v?n chuy?n
    public decimal? GHNTotalFee { get; set; } // T?ng phí v?n chuy?n GHN
    public DateTime? GHNExpectedDelivery { get; set; } // Th?i gian giao d? ki?n
    public string? GHNStatus { get; set; } // Tr?ng thái don hàng trên GHN
    public DateTime? GHNCreatedAt { get; set; } // Th?i gian t?o don trên GHN
    public string? Notes { get; set; } // Ghi chú don hàng
    public string? PaymentMethod { get; set; } // Phuong th?c thanh toán

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();

    public virtual ICollection<OrderAddress> OrderAddresses { get; set; } = new List<OrderAddress>();

    public virtual OrderInvoice? OrderInvoice { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual ICollection<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();

    [JsonIgnore]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [JsonIgnore]
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    [JsonIgnore]
    public virtual User? User { get; set; }

    // Helper property to get shipping address
    public virtual OrderAddress? ShippingAddress => OrderAddresses?.FirstOrDefault(a => a.AddressType.ToLower() == "shipping");
}
