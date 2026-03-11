namespace TheLightStore.Infrastructure.Configuration;

public class GHNSettings
{
    public string Token { get; set; } = null!;
    public string ShopId { get; set; } = null!;
    public bool IsTestMode { get; set; } = true;
    
    // Shop information
    public string ShopName { get; set; } = "TheLightStore";
    public string ShopPhone { get; set; } = "0123456789";
    public string ShopAddress { get; set; } = "123 ABC Street";
    public string ShopWardName { get; set; } = "Ward 1";
    public string ShopDistrictName { get; set; } = "District 1";
    public string ShopProvinceName { get; set; } = "Ho Chi Minh";
}
