namespace TheLightStore.Dtos.Search;
public class OrderSuggestionDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserSuggestionDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class BlogPostSuggestionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
}