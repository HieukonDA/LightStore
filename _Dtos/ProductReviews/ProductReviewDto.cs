using TheLightStore.Dtos.Paging;

namespace TheLightStore.Dtos.ProductReviews;

public class ProductReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? UserId { get; set; }
    public int? OrderId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = null!;
    public List<string>? Images { get; set; }
    public string? Status { get; set; }
    public bool? IsVerifiedPurchase { get; set; }
    public int? HelpfulCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public string? ProductName { get; set; }
    public bool? HasUserVoted { get; set; }
    public bool? UserVoteIsHelpful { get; set; }
}

public class CreateProductReviewDto
{
    public int ProductId { get; set; }
    public int? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = null!;
    public List<string>? Images { get; set; }
}

public class UpdateProductReviewDto
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = null!;
    public List<string>? Images { get; set; }
}

public class ReviewVoteDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int? UserId { get; set; }
    public string? IpAddress { get; set; }
    public bool IsHelpful { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateReviewVoteDto
{
    public int ReviewId { get; set; }
    public bool IsHelpful { get; set; }
}

public class ProductReviewSummaryDto
{
    public int ProductId { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public int TotalHelpfulVotes { get; set; }
}

public class ProductReviewsPagedDto
{
    public PagedResult<ProductReviewDto> Reviews { get; set; } = null!;
    public ProductReviewSummaryDto Summary { get; set; } = null!;
}