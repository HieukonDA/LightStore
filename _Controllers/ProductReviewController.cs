using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Dtos.Paging;
using TheLightStore.Dtos.ProductReviews;
using TheLightStore.Interfaces.ProductReviews;
using TheLightStore.Services;

namespace TheLightStore.Controllers.ProductReviews;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductReviewController : ControllerBase
{
    private readonly IProductReviewService _reviewService;
    private readonly ILogger<ProductReviewController> _logger;

    public ProductReviewController(IProductReviewService reviewService, ILogger<ProductReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đánh giá của một sản phẩm với phân trang
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách đánh giá và thống kê tổng quan</returns>
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductIdAsync(int productId, [FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Getting reviews for product {ProductId}, user: {UserId}", productId, currentUserId);

            var result = await _reviewService.GetByProductIdAsync(productId, pagedRequest, currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting reviews for product {ProductId}", productId);
            return StatusCode(500, ServiceResult<ProductReviewsPagedDto>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy chi tiết một đánh giá
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Chi tiết đánh giá</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _reviewService.GetByIdAsync(id, currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting review {ReviewId}", id);
            return StatusCode(500, ServiceResult<ProductReviewDto>.FailureResult(
                "Đã xảy ra lỗi khi lấy thông tin đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Tạo đánh giá mới cho sản phẩm
    /// </summary>
    /// <param name="dto">Thông tin đánh giá mới</param>
    /// <returns>Thông tin đánh giá đã tạo</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductReviewDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            _logger.LogInformation("Creating review for product {ProductId}, user: {UserId}", dto.ProductId, currentUserId);

            // Nếu user đã đăng nhập, lấy thông tin từ claims
            if (currentUserId.HasValue)
            {
                var userName = GetCurrentUserName();
                var userEmail = GetCurrentUserEmail();
                
                if (string.IsNullOrEmpty(dto.CustomerName))
                    dto.CustomerName = userName;
                if (string.IsNullOrEmpty(dto.CustomerEmail))
                    dto.CustomerEmail = userEmail;
            }

            var result = await _reviewService.CreateAsync(dto, currentUserId, ipAddress);
            
            if (result.Success)
            {
                return Ok(result); // Trả HTTP 200 với dữ liệu review vừa tạo
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating review for product {ProductId}", dto.ProductId);
            return StatusCode(500, ServiceResult<ProductReviewDto>.FailureResult(
                "Đã xảy ra lỗi khi tạo đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Cập nhật đánh giá (chỉ người tạo mới có thể cập nhật)
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <param name="dto">Thông tin cập nhật</param>
    /// <returns>Thông tin đánh giá đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProductReviewDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ServiceResult<ProductReviewDto>.FailureResult(
                    "Unauthorized", new List<string> { "Authentication required" }));
            }

            var result = await _reviewService.UpdateAsync(id, dto, currentUserId);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating review {ReviewId}", id);
            return StatusCode(500, ServiceResult<ProductReviewDto>.FailureResult(
                "Đã xảy ra lỗi khi cập nhật đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Xóa đánh giá (chỉ người tạo mới có thể xóa)
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ServiceResult<bool>.FailureResult(
                    "Unauthorized", new List<string> { "Authentication required" }));
            }

            var result = await _reviewService.DeleteAsync(id, currentUserId);
            
            if (result.Success)
            {
                return NoContent();
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting review {ReviewId}", id);
            return StatusCode(500, ServiceResult<bool>.FailureResult(
                "Đã xảy ra lỗi khi xóa đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Vote "helpful" cho một đánh giá
    /// </summary>
    /// <param name="dto">Thông tin vote</param>
    /// <returns>Thông tin vote đã tạo</returns>
    [HttpPost("vote")]
    public async Task<IActionResult> VoteHelpfulAsync([FromBody] CreateReviewVoteDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            _logger.LogInformation("Adding vote for review {ReviewId}, user: {UserId}", dto.ReviewId, currentUserId);

            var result = await _reviewService.VoteHelpfulAsync(dto, currentUserId, ipAddress);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while voting for review {ReviewId}", dto.ReviewId);
            return StatusCode(500, ServiceResult<ReviewVoteDto>.FailureResult(
                "Đã xảy ra lỗi khi vote đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Xóa vote cho một đánh giá
    /// </summary>
    /// <param name="reviewId">ID của đánh giá</param>
    /// <returns>Kết quả xóa vote</returns>
    [HttpDelete("vote/{reviewId}")]
    public async Task<IActionResult> RemoveVoteAsync(int reviewId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            var result = await _reviewService.RemoveVoteAsync(reviewId, currentUserId, ipAddress);
            
            if (result.Success)
            {
                return NoContent();
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing vote for review {ReviewId}", reviewId);
            return StatusCode(500, ServiceResult<bool>.FailureResult(
                "Đã xảy ra lỗi khi xóa vote", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy thống kê đánh giá của sản phẩm
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <returns>Thống kê đánh giá</returns>
    [HttpGet("product/{productId}/summary")]
    public async Task<IActionResult> GetProductReviewSummaryAsync(int productId)
    {
        try
        {
            var result = await _reviewService.GetProductReviewSummaryAsync(productId);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting review summary for product {ProductId}", productId);
            return StatusCode(500, ServiceResult<ProductReviewSummaryDto>.FailureResult(
                "Đã xảy ra lỗi khi lấy thống kê đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá của người dùng hiện tại
    /// </summary>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách đánh giá của người dùng</returns>
    [HttpGet("my-reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviewsAsync([FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(ServiceResult<PagedResult<ProductReviewDto>>.FailureResult(
                    "Unauthorized", new List<string> { "Authentication required" }));
            }

            var result = await _reviewService.GetUserReviewsAsync(currentUserId.Value, pagedRequest);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user reviews");
            return StatusCode(500, ServiceResult<PagedResult<ProductReviewDto>>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách đánh giá của bạn", 
                new List<string> { "Internal server error" }));
        }
    }

    #region Admin Functions

    /// <summary>
    /// Lấy danh sách đánh giá chờ duyệt (Admin only)
    /// </summary>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách đánh giá chờ duyệt</returns>
    [HttpGet("pending")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetPendingReviewsAsync([FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            var result = await _reviewService.GetPendingReviewsAsync(pagedRequest);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting pending reviews");
            return StatusCode(500, ServiceResult<PagedResult<ProductReviewDto>>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách đánh giá chờ duyệt", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Phê duyệt đánh giá (Admin only)
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Kết quả phê duyệt</returns>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveReviewAsync(int id)
    {
        try
        {
            var result = await _reviewService.ApproveReviewAsync(id);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving review {ReviewId}", id);
            return StatusCode(500, ServiceResult<bool>.FailureResult(
                "Đã xảy ra lỗi khi phê duyệt đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Từ chối đánh giá (Admin only)
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Kết quả từ chối</returns>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectReviewAsync(int id)
    {
        try
        {
            var result = await _reviewService.RejectReviewAsync(id);
            
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting review {ReviewId}", id);
            return StatusCode(500, ServiceResult<bool>.FailureResult(
                "Đã xảy ra lỗi khi từ chối đánh giá", 
                new List<string> { "Internal server error" }));
        }
    }

    #endregion

    #region Private Helper Methods

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return null;
    }

    private string GetCurrentUserName()
    {
        var firstName = User.FindFirst("FirstName")?.Value ?? "";
        var lastName = User.FindFirst("LastName")?.Value ?? "";
        return $"{firstName} {lastName}".Trim();
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    }

    private string GetClientIpAddress()
    {
        // Get IP từ headers (trường hợp có proxy/load balancer)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    #endregion
}