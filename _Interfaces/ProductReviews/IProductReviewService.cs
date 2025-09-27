using TheLightStore.Dtos.ProductReviews;
using TheLightStore.Dtos.Paging;
using TheLightStore.Services;

namespace TheLightStore.Interfaces.ProductReviews;

public interface IProductReviewService
{
    // Review Management
    Task<ServiceResult<ProductReviewDto>> GetByIdAsync(int id, int? currentUserId = null);
    Task<ServiceResult<ProductReviewsPagedDto>> GetByProductIdAsync(int productId, PagedRequest pagedRequest, int? currentUserId = null);
    Task<ServiceResult<ProductReviewDto>> CreateAsync(CreateProductReviewDto dto, int? userId = null, string? ipAddress = null);
    Task<ServiceResult<ProductReviewDto>> UpdateAsync(int id, UpdateProductReviewDto dto, int? userId = null);
    Task<ServiceResult<bool>> DeleteAsync(int id, int? userId = null);
    
    // Review Status Management
    Task<ServiceResult<bool>> ApproveReviewAsync(int id);
    Task<ServiceResult<bool>> RejectReviewAsync(int id);
    
    // Vote Management
    Task<ServiceResult<ReviewVoteDto>> VoteHelpfulAsync(CreateReviewVoteDto dto, int? userId = null, string? ipAddress = null);
    Task<ServiceResult<bool>> RemoveVoteAsync(int reviewId, int? userId = null, string? ipAddress = null);
    
    // Statistics
    Task<ServiceResult<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId);
    Task<ServiceResult<PagedResult<ProductReviewDto>>> GetUserReviewsAsync(int userId, PagedRequest pagedRequest);
    Task<ServiceResult<PagedResult<ProductReviewDto>>> GetPendingReviewsAsync(PagedRequest pagedRequest);
}