using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TheLightStore.Application.DTOs.Products;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.DTOs;

namespace TheLightStore.Controllers.Products;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly Serilog.ILogger _logger;

    public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
    {
        _productService = productService;
        _webHostEnvironment = webHostEnvironment;
        _logger = new LoggerConfiguration()
            .WriteTo.File("logproduct.md", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    #region crud basic

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] PagedRequest pagedRequest)
    {
        _logger.Information("===== GET /api/v1/Product called with Page={Page}, Size={Size} =====", pagedRequest.Page, pagedRequest.Size);
        
        try
        {
            var result = await _productService.GetPagedAsync(pagedRequest);
            
            if (result.Success)
            {
                _logger.Information("Products found: {Count} items", result.Data?.Items?.Count ?? 0);
                return Ok(result);
            }

            _logger.Warning("GetAllProducts failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when getting all products");
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }

    // 🔥 Admin endpoint - lấy tất cả products (bao gồm inactive)
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllForAdminAsync([FromQuery] PagedRequest pagedRequest)
    {
        _logger.Information("===== ADMIN GET /api/v1/Product/admin called with Page={Page}, Size={Size} =====", pagedRequest.Page, pagedRequest.Size);
        
        try
        {
            var result = await _productService.GetPagedForAdminAsync(pagedRequest);
            
            if (result.Success)
            {
                var activeCount = result.Data?.Items?.Count(p => p.IsActive) ?? 0;
                var inactiveCount = (result.Data?.Items?.Count ?? 0) - activeCount;
                _logger.Information("Admin products found: {Total} total ({Active} active, {Inactive} inactive)", 
                    result.Data?.Items?.Count ?? 0, activeCount, inactiveCount);
                return Ok(result);
            }

            _logger.Warning("Admin GetAllProducts failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when getting all products for admin");
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategoryAsync(int categoryId, [FromQuery] PagedRequest pagedRequest)
    {
        var result = await _productService.GetByCategoryAsync(categoryId, pagedRequest);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpGet("category/slug/{slug}")]
    public async Task<IActionResult> GetByCategorySlugAsync(string slug, [FromQuery] PagedRequest pagedRequest)
    {
        var result = await _productService.GetByCategorySlugAsync(slug, pagedRequest);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        _logger.Information("===== GET /api/v1/Product/id/{Id} called =====", id);
        
        try
        {
            var result = await _productService.GetByIdAsync(id);
            
            if (result.Success)
            {
                _logger.Information("Product found: {@Product}", result.Data);
                return Ok(result);
            }

            _logger.Warning("Product with ID {Id} failed: {Errors}", id, string.Join(", ", result.Errors));
            
            // Kiểm tra nếu là "not found" error
            if (result.Errors.Any(e => e.ToLower().Contains("not found")))
            {
                return NotFound(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when getting product with ID {Id}", id);
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }


    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlugAsync(string slug)
    {
        _logger.Information("===== GET /api/v1/Product/slug/{Slug} called =====", slug);
        
        // Thêm validation và logging chi tiết
        if (string.IsNullOrWhiteSpace(slug))
        {
            _logger.Warning("GetBySlugAsync called with null/empty slug");
            return BadRequest(ServiceResult<object>.FailureResult("Invalid slug", new List<string> { "Slug cannot be null or empty" }));
        }

        try
        {
            _logger.Information("Calling ProductService.GetBySlugAsync with slug: '{Slug}'", slug);
            var result = await _productService.GetBySlugAsync(slug);
            _logger.Information("ProductService returned: Success={Success}, HasData={HasData}, ErrorCount={ErrorCount}", 
                result.Success, result.Data != null, result.Errors?.Count ?? 0);

            if (result.Success)
            {
                _logger.Information("Product found: {@Product}", result.Data);
                return Ok(result);
            }

            // 🔥 Sửa logic: kiểm tra xem có phải lỗi "not found" hay lỗi khác
            _logger.Warning("Product with slug {Slug} failed: {Errors}", slug, string.Join(", ", result.Errors ?? new List<string>()));
            
            // Kiểm tra nếu là "not found" error
            if (result.Errors?.Any(e => e?.ToLower().Contains("not found") == true) == true)
            {
                return NotFound(result);
            }
            
            // Các lỗi khác (validation, etc.)
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when getting product with slug {Slug}", slug);
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }


    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductDto dto)
    {
        _logger.Information("===== POST /api/v1/Product called with Name={Name} =====", dto?.Name);
        
        if (dto == null)
        {
            _logger.Warning("CreateProduct called with null DTO");
            return BadRequest(ServiceResult<object>.FailureResult("Invalid request data", new List<string> { "Request body is required" }));
        }
        
        try
        {
            var result = await _productService.CreateAsync(dto);
            
            if (result.Success)
            {
                _logger.Information("Product created successfully: {@Product}", result.Data);
                return Ok(result);
            }

            _logger.Warning("Product creation failed: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when creating product");
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProductDto dto)
    {
        _logger.Information("===== PUT /api/v1/Product/{Id} called =====", id);
        
        try
        {
            var result = await _productService.UpdateAsync(id, dto);
            
            if (result.Success)
            {
                _logger.Information("Product {Id} updated successfully: {@Product}", id, result.Data);
                return Ok(result);
            }

            _logger.Warning("Product {Id} update failed: {Errors}", id, string.Join(", ", result.Errors));
            
            // Kiểm tra nếu là "not found" error
            if (result.Errors.Any(e => e.ToLower().Contains("not found")))
            {
                return NotFound(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when updating product {Id}", id);
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        _logger.Information("===== DELETE /api/v1/Product/{Id} called =====", id);
        
        try
        {
            var result = await _productService.DeleteAsync(id);
            
            if (result.Success)
            {
                _logger.Information("Product {Id} deleted successfully", id);
                return NoContent();
            }

            _logger.Warning("Product {Id} deletion failed: {Errors}", id, string.Join(", ", result.Errors));
            
            // Kiểm tra nếu là "not found" error
            if (result.Errors.Any(e => e.ToLower().Contains("not found")))
            {
                return NotFound(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when deleting product {Id}", id);
            return StatusCode(500, ServiceResult<object>.FailureResult("Internal server error", new List<string> { ex.Message }));
        }
    }

    [HttpPost("upload-image/{productId}")]
    public async Task<IActionResult> UploadImageAsync(int productId, IFormFile file)
    {
        _logger.Information("===== POST /api/v1/Product/upload-image/{ProductId} called =====", productId);
        
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.Warning("Upload image failed: No file provided for product {ProductId}", productId);
                return BadRequest(ServiceResult<string>.FailureResult("No file uploaded", new List<string> { "File is required" }));
            }

            // Validate productId
            if (productId <= 0)
            {
                return BadRequest(ServiceResult<string>.FailureResult("Invalid product ID", new List<string> { "Product ID is required" }));
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(ServiceResult<string>.FailureResult("Invalid file type. Only JPEG, PNG, GIF, WEBP are allowed", new List<string> { "Invalid file type" }));
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(ServiceResult<string>.FailureResult("File size cannot exceed 5MB", new List<string> { "File size is too large" }));
            }

            // ✅ Tạo thư mục theo cấu trúc: products/product{id}/
            var productFolder = $"product{productId}";
            var relativePath = Path.Combine("products", productFolder);
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            
            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // ✅ Tìm số thứ tự tiếp theo cho ảnh
            var existingFiles = Directory.GetFiles(uploadsPath, "*.*")
                .Where(f => new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }
                    .Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            int nextNumber = 1;
            if (existingFiles.Any())
            {
                var numbers = existingFiles
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .Where(name => int.TryParse(name, out _))
                    .Select(int.Parse);
                
                if (numbers.Any())
                {
                    nextNumber = numbers.Max() + 1;
                }
            }

            // ✅ Tạo tên file theo số thứ tự
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{nextNumber}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Return URL theo cấu trúc thực tế
            var imageUrl = $"/{relativePath.Replace("\\", "/")}/{fileName}";
            
            _logger.Information("Image uploaded successfully for product {ProductId}: {ImageUrl}", productId, imageUrl);
            return Ok(ServiceResult<string>.SuccessResult(imageUrl));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when uploading image for product {ProductId}", productId);
            return StatusCode(500, ServiceResult<string>.FailureResult($"Upload failed: {ex.Message}", new List<string> { "Upload failed" }));
        }
    }

    [HttpDelete("delete-image")]
    public Task<IActionResult> DeleteImageAsync([FromQuery] string imageUrl)
    {
        _logger.Information("===== DELETE /api/v1/Product/delete-image called with URL={ImageUrl} =====", imageUrl);
        
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.Warning("Delete image failed: No URL provided");
                return Task.FromResult<IActionResult>(BadRequest(ServiceResult<bool>.FailureResult("Image URL is required", new List<string> { "Image URL is required" })));
            }

            // Remove leading slash if present
            var relativePath = imageUrl.TrimStart('/');
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                _logger.Information("Image deleted successfully: {ImageUrl}", imageUrl);
                return Task.FromResult<IActionResult>(Ok(ServiceResult<bool>.SuccessResult(true)));
            }

            _logger.Warning("Image not found for deletion: {ImageUrl}", imageUrl);
            return Task.FromResult<IActionResult>(NotFound(ServiceResult<bool>.FailureResult("File not found", new List<string> { "File not found" })));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception when deleting image {ImageUrl}", imageUrl);
            return Task.FromResult<IActionResult>(StatusCode(500, ServiceResult<bool>.FailureResult($"Delete failed: {ex.Message}", new List<string> { "Delete failed" })));
        }
    }

    #endregion
}