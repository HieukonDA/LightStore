namespace TheLightStore.Dtos.Orders;

public class OrderAddressDto
{
    public string AddressType { get; set; } = null!; // shipping | billing

    public string RecipientName { get; set; } = null!;
    public string Phone { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }

    public string Ward { get; set; } = null!;
    public string District { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Province { get; set; } = null!;

    public string? PostalCode { get; set; }
}
