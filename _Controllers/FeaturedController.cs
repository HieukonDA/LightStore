using Microsoft.AspNetCore.Mvc;
using TheLightStore.Interfaces.Products;
using TheLightStore.Dtos.Paging;
using TheLightStore.Dtos.Products;
using TheLightStore.Services;

namespace TheLightStore.Controllers.Products;

[ApiController]
[Route("api/v1/products/[controller]")]
public class FeaturedController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<FeaturedController> _logger;

    public FeaturedController(IProductService productService, ILogger<FeaturedController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm nổi bật với phân trang
    /// </summary>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách sản phẩm nổi bật</returns>
    [HttpGet]
    public async Task<IActionResult> GetFeaturedProductsAsync([FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Getting featured products with page {Page}, size {Size}", 
                pagedRequest.Page, pagedRequest.Size);

            var result = await _productService.GetFeaturedAsync(pagedRequest);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully retrieved {TotalCount} featured products", 
                    result.Data?.TotalCount ?? 0);
                return Ok(result);
            }

            _logger.LogWarning("Failed to get featured products: {Errors}", 
                string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting featured products");
            return StatusCode(500, ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách sản phẩm nổi bật", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm mới với phân trang
    /// </summary>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách sản phẩm mới</returns>
    [HttpGet("new")]
    public async Task<IActionResult> GetNewProductsAsync([FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            _logger.LogInformation("Getting new products with page {Page}, size {Size}", 
                pagedRequest.Page, pagedRequest.Size);

            var result = await _productService.GetNewProductsAsync(pagedRequest);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully retrieved {TotalCount} new products", 
                    result.Data?.TotalCount ?? 0);
                return Ok(result);
            }

            _logger.LogWarning("Failed to get new products: {Errors}", 
                string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting new products");
            return StatusCode(500, ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách sản phẩm mới", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm liên quan cùng danh mục với phân trang
    /// </summary>
    /// <param name="productId">ID của sản phẩm gốc</param>
    /// <param name="pagedRequest">Thông tin phân trang</param>
    /// <returns>Danh sách sản phẩm liên quan</returns>
    [HttpGet("related/{productId}")]
    public async Task<IActionResult> GetRelatedProductsAsync(int productId, [FromQuery] PagedRequest pagedRequest)
    {
        try
        {
            if (productId <= 0)
            {
                return BadRequest(ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    "ID sản phẩm không hợp lệ", 
                    new List<string> { "Product ID must be greater than 0" }));
            }

            _logger.LogInformation("Getting related products for product {ProductId} with page {Page}, size {Size}", 
                productId, pagedRequest.Page, pagedRequest.Size);

            var result = await _productService.GetRelatedAsync(productId, pagedRequest);
            
            if (result.Success)
            {
                _logger.LogInformation("Successfully retrieved {TotalCount} related products for product {ProductId}", 
                    result.Data?.TotalCount ?? 0, productId);
                return Ok(result);
            }

            _logger.LogWarning("Failed to get related products for product {ProductId}: {Errors}", 
                productId, string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting related products for product {ProductId}", productId);
            return StatusCode(500, ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                "Đã xảy ra lỗi khi lấy danh sách sản phẩm liên quan", 
                new List<string> { "Internal server error" }));
        }
    }

    /// <summary>
    /// Lấy thống kê tổng quan sản phẩm nổi bật
    /// </summary>
    /// <returns>Thống kê sản phẩm nổi bật</returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetFeaturedStatsAsync()
    {
        try
        {
            _logger.LogInformation("Getting featured products statistics");

            // Lấy một trang nhỏ để tính thống kê
            var statsRequest = new PagedRequest { Page = 1, Size = 1 };
            var result = await _productService.GetFeaturedAsync(statsRequest);
            
            if (result.Success)
            {
                var stats = new
                {
                    TotalFeaturedProducts = result.Data?.TotalCount ?? 0,
                    TotalPages = result.Data?.TotalPages ?? 0,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully retrieved featured products stats: {TotalFeatured} products", 
                    stats.TotalFeaturedProducts);
                
                return Ok(ServiceResult<object>.SuccessResult(stats));
            }

            _logger.LogWarning("Failed to get featured products stats: {Errors}", 
                string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting featured products statistics");
            return StatusCode(500, ServiceResult<object>.FailureResult(
                "Đã xảy ra lỗi khi lấy thống kê sản phẩm nổi bật", 
                new List<string> { "Internal server error" }));
        }
    }
}