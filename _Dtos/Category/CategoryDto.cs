namespace TheLightStore.Dtos.Category;
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int? ParentId { get; set; } = 0;
    public string ParentName { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Hierarchical data
    public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();
    public int ProductCount { get; set; } = 0;
}