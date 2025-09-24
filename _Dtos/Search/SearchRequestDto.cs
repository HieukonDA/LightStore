namespace TheLightStore.Dtos.Search;
// Base search request
public abstract class BaseSearchRequest : PagedRequest
{
    public string Query { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
} 

// Product search
public class SearchProductsRequest : BaseSearchRequest
{
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Relevance;
}

// Order search (for Admin)
public class SearchOrdersRequest : BaseSearchRequest
{
    public string? Status { get; set; }
    public int? UserId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

// User search (for Admin)
public class SearchUsersRequest : BaseSearchRequest
{
    public UserStatus? Status { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsEmailVerified { get; set; }
}