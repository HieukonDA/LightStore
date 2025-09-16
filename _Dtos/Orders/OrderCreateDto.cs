namespace TheLightStore.DTOs.Orders;

public class OrderCreateDto
{
    public int? UserId { get; set; } // Có thể null nếu guest checkout

    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;

    public string? CustomerNotes { get; set; }

    public decimal ShippingCost { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? DiscountAmount { get; set; }

    public string PaymentMethod { get; set; } = null!; 
    // ví dụ: "cod", "momo", "vnpay"

    public List<OrderItemCreateDto> Items { get; set; } = new();

    public OrderAddressCreateDto ShippingAddress { get; set; } = null!;
}
