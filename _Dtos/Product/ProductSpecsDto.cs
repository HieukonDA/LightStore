namespace TheLightStore.Dtos.Products;
public class ProductSpecsDto
{
    public string? PowerConsumption { get; set; }
    public string? LightColor { get; set; }
    public string? LightOutput { get; set; }
    public string? IPRating { get; set; }
    public string? BeamAngle { get; set; }
    public bool IsDimmable { get; set; }
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? Origin { get; set; }
    public string? WarrantyType { get; set; }
    public int WarrantyPeriod { get; set; }
}
