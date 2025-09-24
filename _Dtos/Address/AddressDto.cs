namespace TheLightStore.Dtos.Addresses;

/// <summary>
/// DTO for returning address information
/// </summary>
public class AddressDto
{
    public int Id { get; set; }
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
    public bool? IsDefault { get; set; }
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Full formatted address string
    /// </summary>
    public string FullAddress => $"{AddressLine1}{(!string.IsNullOrEmpty(AddressLine2) ? ", " + AddressLine2 : "")}, {Ward}, {District}, {City}, {Province}";
}