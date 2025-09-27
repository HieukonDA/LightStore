using System.Text.Json;
using TheLightStore.Dtos.Paging;
using TheLightStore.Dtos.ProductReviews;
using TheLightStore.Interfaces.ProductReviews;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.ProductReviews;
using TheLightStore.Services;

namespace TheLightStore.Services.ProductReviews;

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepo _reviewRepo;
    private readonly IProductRepo _productRepo;
    private readonly ILogger<ProductReviewService> _logger;

    public ProductReviewService(
        IProductReviewRepo reviewRepo,
        IProductRepo productRepo,
        ILogger<ProductReviewService> logger)
    {
        _reviewRepo = reviewRepo;
        _productRepo = productRepo;
        _logger = logger;
    }

    #region Review Management

    public async Task<ServiceResult<ProductReviewDto>> GetByIdAsync(int id, int? currentUserId = null)
    {
        try
        {
            _logger.LogInformation("Getting review with ID {ReviewId}", id);

            if (id <= 0)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Invalid review ID", new List<string> { "Review ID must be greater than 0" });
            }

            var review = await _reviewRepo.GetByIdAsync(id, true);
            if (review == null)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            var dto = await MapToReviewDto(review, currentUserId);
            return ServiceResult<ProductReviewDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting review {ReviewId}", id);
            return ServiceResult<ProductReviewDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<ProductReviewsPagedDto>> GetByProductIdAsync(int productId, PagedRequest pagedRequest, int? currentUserId = null)
    {
        try
        {
            _logger.LogInformation("Getting reviews for product {ProductId}", productId);

            if (productId <= 0)
            {
                return ServiceResult<ProductReviewsPagedDto>.FailureResult("Invalid product ID", new List<string> { "Product ID must be greater than 0" });
            }

            // Validate paged request
            var validationResult = ValidatePagedRequest(pagedRequest);
            if (!validationResult.Success)
            {
                return ServiceResult<ProductReviewsPagedDto>.FailureResult("Invalid paged request", validationResult.Errors);
            }

            // Check if product exists
            var productExists = await _productRepo.ExistsAsync(productId);
            if (!productExists)
            {
                return ServiceResult<ProductReviewsPagedDto>.FailureResult("Product not found", new List<string> { "Product does not exist" });
            }

            // Get reviews
            var reviewsResult = await _reviewRepo.GetByProductIdAsync(productId, pagedRequest);
            var reviewDtos = new List<ProductReviewDto>();

            foreach (var review in reviewsResult.Items)
            {
                var dto = await MapToReviewDto(review, currentUserId);
                reviewDtos.Add(dto);
            }

            var pagedResult = new PagedResult<ProductReviewDto>
            {
                Items = reviewDtos,
                TotalCount = reviewsResult.TotalCount,
                Page = reviewsResult.Page,
                PageSize = reviewsResult.PageSize
            };

            // Get summary
            var summary = await GetProductReviewSummaryAsync(productId);
            if (!summary.Success)
            {
                return ServiceResult<ProductReviewsPagedDto>.FailureResult("Error getting review summary", summary.Errors);
            }

            var result = new ProductReviewsPagedDto
            {
                Reviews = pagedResult,
                Summary = summary.Data!
            };

            return ServiceResult<ProductReviewsPagedDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting reviews for product {ProductId}", productId);
            return ServiceResult<ProductReviewsPagedDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> CreateAsync(CreateProductReviewDto dto, int? userId = null, string? ipAddress = null)
    {
        try
        {
            _logger.LogInformation("Creating review for product {ProductId}", dto.ProductId);

            // Validate input
            var validationResult = ValidateCreateReviewDto(dto);
            if (!validationResult.Success)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Validation failed", validationResult.Errors);
            }

            // Check if product exists
            var productExists = await _productRepo.ExistsAsync(dto.ProductId);
            if (!productExists)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Product not found", new List<string> { "Product does not exist" });
            }

            // Check if user already reviewed this product (if user is logged in)
            if (userId.HasValue)
            {
                var hasReviewed = await _reviewRepo.HasUserReviewedProductAsync(userId.Value, dto.ProductId);
                if (hasReviewed)
                {
                    return ServiceResult<ProductReviewDto>.FailureResult("Already reviewed", new List<string> { "You have already reviewed this product" });
                }
            }

            // Check if user purchased the product (for verified purchase)
            bool isVerifiedPurchase = false;
            if (userId.HasValue)
            {
                isVerifiedPurchase = await _reviewRepo.HasUserPurchasedProductAsync(userId.Value, dto.ProductId);
            }

            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                UserId = userId,
                OrderId = dto.OrderId,
                CustomerName = dto.CustomerName ?? "Anonymous",
                CustomerEmail = dto.CustomerEmail ?? "",
                Rating = dto.Rating,
                Title = dto.Title,
                Comment = dto.Comment,
                Images = dto.Images != null && dto.Images.Any() ? JsonSerializer.Serialize(dto.Images) : null,
                Status = "approved", // Reviews need approval by default
                IsVerifiedPurchase = isVerifiedPurchase,
                HelpfulCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepo.AddAsync(review);
            var resultDto = await MapToReviewDto(createdReview, userId);

            _logger.LogInformation("Successfully created review {ReviewId} for product {ProductId}", createdReview.Id, dto.ProductId);
            return ServiceResult<ProductReviewDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating review for product {ProductId}", dto.ProductId);
            return ServiceResult<ProductReviewDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> UpdateAsync(int id, UpdateProductReviewDto dto, int? userId = null)
    {
        try
        {
            _logger.LogInformation("Updating review {ReviewId}", id);

            if (id <= 0)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Invalid review ID", new List<string> { "Review ID must be greater than 0" });
            }

            // Validate input
            var validationResult = ValidateUpdateReviewDto(dto);
            if (!validationResult.Success)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Validation failed", validationResult.Errors);
            }

            var existingReview = await _reviewRepo.GetByIdAsync(id);
            if (existingReview == null)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            // Check if user owns this review
            if (userId.HasValue && existingReview.UserId != userId.Value)
            {
                return ServiceResult<ProductReviewDto>.FailureResult("Unauthorized", new List<string> { "You can only update your own reviews" });
            }

            existingReview.Rating = dto.Rating;
            existingReview.Title = dto.Title;
            existingReview.Comment = dto.Comment;
            existingReview.Images = dto.Images != null && dto.Images.Any() ? JsonSerializer.Serialize(dto.Images) : null;
            existingReview.Status = "pending"; // Reset to pending after update

            var updatedReview = await _reviewRepo.UpdateAsync(existingReview);
            var resultDto = await MapToReviewDto(updatedReview, userId);

            _logger.LogInformation("Successfully updated review {ReviewId}", id);
            return ServiceResult<ProductReviewDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating review {ReviewId}", id);
            return ServiceResult<ProductReviewDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int? userId = null)
    {
        try
        {
            _logger.LogInformation("Deleting review {ReviewId}", id);

            if (id <= 0)
            {
                return ServiceResult<bool>.FailureResult("Invalid review ID", new List<string> { "Review ID must be greater than 0" });
            }

            var existingReview = await _reviewRepo.GetByIdAsync(id);
            if (existingReview == null)
            {
                return ServiceResult<bool>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            // Check if user owns this review
            if (userId.HasValue && existingReview.UserId != userId.Value)
            {
                return ServiceResult<bool>.FailureResult("Unauthorized", new List<string> { "You can only delete your own reviews" });
            }

            var result = await _reviewRepo.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Successfully deleted review {ReviewId}", id);
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.FailureResult("Delete failed", new List<string> { "Failed to delete review" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting review {ReviewId}", id);
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    #endregion

    #region Review Status Management

    public async Task<ServiceResult<bool>> ApproveReviewAsync(int id)
    {
        try
        {
            _logger.LogInformation("Approving review {ReviewId}", id);

            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                return ServiceResult<bool>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            review.Status = "approved";
            review.ApprovedAt = DateTime.UtcNow;

            await _reviewRepo.UpdateAsync(review);

            _logger.LogInformation("Successfully approved review {ReviewId}", id);
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving review {ReviewId}", id);
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> RejectReviewAsync(int id)
    {
        try
        {
            _logger.LogInformation("Rejecting review {ReviewId}", id);

            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
            {
                return ServiceResult<bool>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            review.Status = "rejected";

            await _reviewRepo.UpdateAsync(review);

            _logger.LogInformation("Successfully rejected review {ReviewId}", id);
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting review {ReviewId}", id);
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    #endregion

    #region Vote Management

    public async Task<ServiceResult<ReviewVoteDto>> VoteHelpfulAsync(CreateReviewVoteDto dto, int? userId = null, string? ipAddress = null)
    {
        try
        {
            _logger.LogInformation("Adding vote for review {ReviewId}", dto.ReviewId);

            if (dto.ReviewId <= 0)
            {
                return ServiceResult<ReviewVoteDto>.FailureResult("Invalid review ID", new List<string> { "Review ID must be greater than 0" });
            }

            // Check if review exists
            var review = await _reviewRepo.GetByIdAsync(dto.ReviewId);
            if (review == null)
            {
                return ServiceResult<ReviewVoteDto>.FailureResult("Review not found", new List<string> { "Review does not exist" });
            }

            // Check if user already voted on this review
            var existingVote = await _reviewRepo.GetVoteAsync(dto.ReviewId, userId, ipAddress);
            if (existingVote != null)
            {
                // Update existing vote
                existingVote.IsHelpful = dto.IsHelpful;
                var updatedVote = new ReviewHelpfulVote
                {
                    Id = existingVote.Id,
                    ReviewId = existingVote.ReviewId,
                    UserId = existingVote.UserId,
                    IpAddress = existingVote.IpAddress,
                    IsHelpful = dto.IsHelpful,
                    CreatedAt = existingVote.CreatedAt
                };

                await _reviewRepo.UpdateHelpfulCountAsync(dto.ReviewId);

                var updatedVoteDto = MapToVoteDto(updatedVote);
                return ServiceResult<ReviewVoteDto>.SuccessResult(updatedVoteDto);
            }

            // Create new vote
            var vote = new ReviewHelpfulVote
            {
                ReviewId = dto.ReviewId,
                UserId = userId,
                IpAddress = ipAddress,
                IsHelpful = dto.IsHelpful,
                CreatedAt = DateTime.UtcNow
            };

            var createdVote = await _reviewRepo.AddVoteAsync(vote);
            var voteDto = MapToVoteDto(createdVote);

            _logger.LogInformation("Successfully added vote for review {ReviewId}", dto.ReviewId);
            return ServiceResult<ReviewVoteDto>.SuccessResult(voteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding vote for review {ReviewId}", dto.ReviewId);
            return ServiceResult<ReviewVoteDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> RemoveVoteAsync(int reviewId, int? userId = null, string? ipAddress = null)
    {
        try
        {
            _logger.LogInformation("Removing vote for review {ReviewId}", reviewId);

            var result = await _reviewRepo.RemoveVoteAsync(reviewId, userId, ipAddress);
            if (result)
            {
                _logger.LogInformation("Successfully removed vote for review {ReviewId}", reviewId);
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.FailureResult("Vote not found", new List<string> { "No vote found to remove" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing vote for review {ReviewId}", reviewId);
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    #endregion

    #region Statistics

    public async Task<ServiceResult<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId)
    {
        try
        {
            _logger.LogInformation("Getting review summary for product {ProductId}", productId);

            if (productId <= 0)
            {
                return ServiceResult<ProductReviewSummaryDto>.FailureResult("Invalid product ID", new List<string> { "Product ID must be greater than 0" });
            }

            var totalReviews = await _reviewRepo.GetTotalReviewsAsync(productId);
            var averageRating = await _reviewRepo.GetAverageRatingAsync(productId);
            var ratingDistribution = await _reviewRepo.GetRatingDistributionAsync(productId);

            var summary = new ProductReviewSummaryDto
            {
                ProductId = productId,
                TotalReviews = totalReviews,
                AverageRating = averageRating,
                RatingDistribution = ratingDistribution,
                TotalHelpfulVotes = 0 // Could be calculated if needed
            };

            return ServiceResult<ProductReviewSummaryDto>.SuccessResult(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting review summary for product {ProductId}", productId);
            return ServiceResult<ProductReviewSummaryDto>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<PagedResult<ProductReviewDto>>> GetUserReviewsAsync(int userId, PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Getting reviews for user {UserId}", userId);

            if (userId <= 0)
            {
                return ServiceResult<PagedResult<ProductReviewDto>>.FailureResult("Invalid user ID", new List<string> { "User ID must be greater than 0" });
            }

            // Validate paged request
            var validationResult = ValidatePagedRequest(pagedRequest);
            if (!validationResult.Success)
            {
                return ServiceResult<PagedResult<ProductReviewDto>>.FailureResult("Invalid paged request", validationResult.Errors);
            }

            var result = await _reviewRepo.GetUserReviewsAsync(userId, pagedRequest);
            var reviewDtos = new List<ProductReviewDto>();

            foreach (var review in result.Items)
            {
                var dto = await MapToReviewDto(review, userId);
                reviewDtos.Add(dto);
            }

            var pagedResult = new PagedResult<ProductReviewDto>
            {
                Items = reviewDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return ServiceResult<PagedResult<ProductReviewDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting reviews for user {UserId}", userId);
            return ServiceResult<PagedResult<ProductReviewDto>>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<PagedResult<ProductReviewDto>>> GetPendingReviewsAsync(PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Getting pending reviews");

            // Validate paged request
            var validationResult = ValidatePagedRequest(pagedRequest);
            if (!validationResult.Success)
            {
                return ServiceResult<PagedResult<ProductReviewDto>>.FailureResult("Invalid paged request", validationResult.Errors);
            }

            var result = await _reviewRepo.GetPendingReviewsAsync(pagedRequest);
            var reviewDtos = new List<ProductReviewDto>();

            foreach (var review in result.Items)
            {
                var dto = await MapToReviewDto(review, null);
                reviewDtos.Add(dto);
            }

            var pagedResult = new PagedResult<ProductReviewDto>
            {
                Items = reviewDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return ServiceResult<PagedResult<ProductReviewDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting pending reviews");
            return ServiceResult<PagedResult<ProductReviewDto>>.FailureResult("An error occurred while processing your request", new List<string> { ex.Message });
        }
    }

    #endregion

    #region Private Methods

    private async Task<ProductReviewDto> MapToReviewDto(ProductReview review, int? currentUserId)
    {
        var images = new List<string>();
        if (!string.IsNullOrEmpty(review.Images))
        {
            try
            {
                images = JsonSerializer.Deserialize<List<string>>(review.Images) ?? new List<string>();
            }
            catch
            {
                images = new List<string>();
            }
        }

        bool? hasUserVoted = null;
        bool? userVoteIsHelpful = null;

        if (currentUserId.HasValue)
        {
            var userVote = await _reviewRepo.GetVoteAsync(review.Id, currentUserId, null);
            if (userVote != null)
            {
                hasUserVoted = true;
                userVoteIsHelpful = userVote.IsHelpful;
            }
            else
            {
                hasUserVoted = false;
            }
        }

        return new ProductReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            OrderId = review.OrderId,
            CustomerName = review.CustomerName,
            CustomerEmail = review.CustomerEmail,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            Images = images,
            Status = review.Status,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            HelpfulCount = review.HelpfulCount,
            CreatedAt = review.CreatedAt,
            ApprovedAt = review.ApprovedAt,
            ProductName = review.Product?.Name,
            HasUserVoted = hasUserVoted,
            UserVoteIsHelpful = userVoteIsHelpful
        };
    }

    private static ReviewVoteDto MapToVoteDto(ReviewHelpfulVote vote)
    {
        return new ReviewVoteDto
        {
            Id = vote.Id,
            ReviewId = vote.ReviewId,
            UserId = vote.UserId,
            IpAddress = vote.IpAddress,
            IsHelpful = vote.IsHelpful,
            CreatedAt = vote.CreatedAt
        };
    }

    private static ServiceResult<bool> ValidatePagedRequest(PagedRequest pagedRequest)
    {
        var errors = new List<string>();

        if (pagedRequest == null)
        {
            errors.Add("Paged request cannot be null");
            return ServiceResult<bool>.FailureResult("Invalid paged request", errors);
        }

        if (pagedRequest.Page < 1)
            errors.Add("Page must be greater than 0");

        if (pagedRequest.Size < 1 || pagedRequest.Size > 100)
            errors.Add("Page size must be between 1 and 100");

        return errors.Any()
            ? ServiceResult<bool>.FailureResult("Invalid paged request", errors)
            : ServiceResult<bool>.SuccessResult(true);
    }

    private static ServiceResult<bool> ValidateCreateReviewDto(CreateProductReviewDto dto)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("Review data cannot be null");
            return ServiceResult<bool>.FailureResult("Invalid review data", errors);
        }

        if (dto.ProductId <= 0)
            errors.Add("Product ID must be greater than 0");

        if (dto.Rating < 1 || dto.Rating > 5)
            errors.Add("Rating must be between 1 and 5");

        if (string.IsNullOrWhiteSpace(dto.Comment))
            errors.Add("Comment is required");
        else if (dto.Comment.Length > 2000)
            errors.Add("Comment cannot exceed 2000 characters");

        if (!string.IsNullOrEmpty(dto.Title) && dto.Title.Length > 200)
            errors.Add("Title cannot exceed 200 characters");

        return errors.Any()
            ? ServiceResult<bool>.FailureResult("Invalid review data", errors)
            : ServiceResult<bool>.SuccessResult(true);
    }

    private static ServiceResult<bool> ValidateUpdateReviewDto(UpdateProductReviewDto dto)
    {
        var errors = new List<string>();

        if (dto == null)
        {
            errors.Add("Review data cannot be null");
            return ServiceResult<bool>.FailureResult("Invalid review data", errors);
        }

        if (dto.Rating < 1 || dto.Rating > 5)
            errors.Add("Rating must be between 1 and 5");

        if (string.IsNullOrWhiteSpace(dto.Comment))
            errors.Add("Comment is required");
        else if (dto.Comment.Length > 2000)
            errors.Add("Comment cannot exceed 2000 characters");

        if (!string.IsNullOrEmpty(dto.Title) && dto.Title.Length > 200)
            errors.Add("Title cannot exceed 200 characters");

        return errors.Any()
            ? ServiceResult<bool>.FailureResult("Invalid review data", errors)
            : ServiceResult<bool>.SuccessResult(true);
    }

    #endregion
}