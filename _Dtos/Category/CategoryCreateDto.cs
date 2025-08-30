public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ParentId { get; set; } = 0;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}