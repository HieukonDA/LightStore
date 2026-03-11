namespace TheLightStore.Infrastructure.ExternalServices;

public class GHNAddressMapping
{
    public int ProvinceId { get; set; }
    public int DistrictId { get; set; }
    public string WardCode { get; set; } = null!;
}
