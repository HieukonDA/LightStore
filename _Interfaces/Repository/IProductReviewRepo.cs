using TheLightStore.Models.ProductReviews;
using TheLightStore.Dtos.Paging;

namespace TheLightStore.Interfaces.Repository;

public interface IProductReviewRepo
{
    // Basic CRUD
    Task<ProductReview?> GetByIdAsync(int id, bool includeRelated = false);
    Task<PagedResult<ProductReview>> GetByProductIdAsync(int productId, PagedRequest pagedRequest);
    Task<ProductReview> AddAsync(ProductReview review);
    Task<ProductReview> UpdateAsync(ProductReview review);
    Task<bool> DeleteAsync(int id);
    
    // Review queries
    Task<PagedResult<ProductReview>> GetUserReviewsAsync(int userId, PagedRequest pagedRequest);
    Task<PagedResult<ProductReview>> GetPendingReviewsAsync(PagedRequest pagedRequest);
    Task<bool> HasUserReviewedProductAsync(int userId, int productId);
    Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
    
    // Statistics
    Task<decimal> GetAverageRatingAsync(int productId);
    Task<int> GetTotalReviewsAsync(int productId);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(int productId);
    
    // Vote management
    Task<ReviewHelpfulVote?> GetVoteAsync(int reviewId, int? userId, string? ipAddress);
    Task<ReviewHelpfulVote> AddVoteAsync(ReviewHelpfulVote vote);
    Task<bool> RemoveVoteAsync(int reviewId, int? userId, string? ipAddress);
    Task<bool> UpdateHelpfulCountAsync(int reviewId);
}