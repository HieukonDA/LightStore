namespace TheLightStore.Models.Orders_Carts;

public partial class OrderPayment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentStatus { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? TransactionId { get; set; }

    public string? GatewayResponse { get; set; }
    public string  PaymentRequestId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
