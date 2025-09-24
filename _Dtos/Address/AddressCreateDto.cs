namespace TheLightStore.Dtos.Addresses;

/// <summary>
/// DTO for creating a new address
/// </summary>
public class AddressCreateDto
{
    public int UserId { get; set; }
    public string AddressType { get; set; } = null!;
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