namespace TheLightStore.Dtos.Orders;

public class OrderPaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? TransactionId { get; set; }
    public Guid PaymentRequestId { get; set; }

    // ✅ Runtime only
    public string? CheckoutUrl { get; set; }
    public string? QrCodeUrl { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}