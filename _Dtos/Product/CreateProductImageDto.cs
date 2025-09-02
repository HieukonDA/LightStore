namespace TheLightStore.Dtos.Products;
public class CreateProductImageDto
{
    public string ImageUrl { get; set; } = null!;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
