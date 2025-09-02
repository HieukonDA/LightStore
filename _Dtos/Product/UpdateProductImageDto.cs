namespace TheLightStore.Dtos.Products;

public class UpdateProductImageDto
{
    public int? Id { get; set; }
    public string? Url { get; set; }
    public string? AltText { get; set; }
    public bool? IsThumbnail { get; set; }
    public int? SortOrder { get; set; }
    public bool IsDeleted { get; set; } = false;
}
