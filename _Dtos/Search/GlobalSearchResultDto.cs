namespace TheLightStore.Dtos.Search;
public class GlobalSearchResultDto
{
    public List<ProductSuggestionDto> Products { get; set; } = new();
    public List<OrderSuggestionDto> Orders { get; set; } = new();
    public List<UserSuggestionDto> Users { get; set; } = new();
    public List<BlogPostSuggestionDto> BlogPosts { get; set; } = new();
    public int TotalResults { get; set; }
}