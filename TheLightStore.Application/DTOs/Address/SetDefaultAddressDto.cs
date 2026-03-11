namespace TheLightStore.Application.DTOs.Address;

/// <summary>
/// DTO for setting default address
/// </summary>
public class SetDefaultAddressDto
{
    public int UserId { get; set; }
    public int AddressId { get; set; }
}