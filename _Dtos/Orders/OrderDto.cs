namespace TheLightStore.Dtos.Orders;


public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string OrderStatus { get; set; } = null!;
    public DateTime OrderDate { get; set; }

    public decimal Subtotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    public OrderAddressDto ShippingAddress { get; set; } = null!;

    public List<OrderItemDto> Items { get; set; } = new();

    public OrderPaymentDto Payment { get; set; } = null!;
}