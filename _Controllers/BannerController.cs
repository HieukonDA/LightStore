using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Dtos.Banners;
using TheLightStore.Interfaces.Banners;

namespace TheLightStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BannerController : ControllerBase
{
    private readonly IBannerService _bannerService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BannerController(IBannerService bannerService, IWebHostEnvironment webHostEnvironment)
    {
        _bannerService = bannerService;
        _webHostEnvironment = webHostEnvironment;
    }

    #region Public Methods (No Authentication Required)

    /// <summary>
    /// Lấy danh sách banner đang hoạt động cho công chúng
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetActiveBanners([FromQuery] string? position = null)
    {
        var response = await _bannerService.GetActiveBannersAsync(position);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    #endregion

    #region Admin/Manager Methods (Authentication Required)

    /// <summary>
    /// Lấy danh sách tất cả banner (Admin/Manager)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBanners(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? position = null,
        [FromQuery] bool? isActive = null)
    {
        var response = await _bannerService.GetBannersAsync(pageNumber, pageSize, position, isActive);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy banner theo ID (Admin/Manager)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBannerById(int id)
    {
        var response = await _bannerService.GetBannerByIdAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Tạo banner mới (Admin/Manager)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int createdBy))
        {
            return Unauthorized(new { Success = false, Message = "Invalid user" });
        }

        var response = await _bannerService.CreateBannerAsync(createDto, createdBy);
        
        if (response.Success)
            return CreatedAtAction(nameof(GetBannerById), new { id = response.Data!.Id }, response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Cập nhật banner (Admin/Manager)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateBanner(int id, [FromBody] UpdateBannerDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var response = await _bannerService.UpdateBannerAsync(id, updateDto);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Xóa banner (Admin/Manager)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var response = await _bannerService.DeleteBannerAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Bật/tắt trạng thái banner (Admin/Manager)
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ToggleBannerStatus(int id)
    {
        var response = await _bannerService.ToggleBannerStatusAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    #endregion

    #region Image Management

    /// <summary>
    /// Upload ảnh cho banner (Admin/Manager)
    /// </summary>
    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadBannerImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "No file uploaded", Errors = new[] { "File is required" } });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/jpg" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { Success = false, Message = "Invalid file type. Only JPEG, PNG, GIF, WEBP are allowed", Errors = new[] { "Invalid file type" } });
            }

            // Validate file size (max 10MB for banners)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { Success = false, Message = "File size cannot exceed 10MB", Errors = new[] { "File size is too large" } });
            }

            // Tạo thư mục banner nếu chưa có
            var bannerFolder = "banner";
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, bannerFolder);
            
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Tạo tên file duy nhất
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"banner_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var imageUrl = $"/{bannerFolder}/{fileName}";
            
            return Ok(new { Success = true, Data = imageUrl, Message = "Banner image uploaded successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Upload failed: {ex.Message}", Errors = new[] { "Upload failed" } });
        }
    }

    /// <summary>
    /// Xóa ảnh banner (Admin/Manager)
    /// </summary>
    [HttpDelete("delete-image")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult DeleteBannerImage([FromQuery] string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { Success = false, Message = "Image URL is required", Errors = new[] { "Image URL is required" } });
            }

            // Kiểm tra xem URL có phải là banner image không
            if (!imageUrl.Contains("/banner/"))
            {
                return BadRequest(new { Success = false, Message = "Invalid banner image URL", Errors = new[] { "Invalid banner image URL" } });
            }

            // Remove leading slash if present và lấy relative path
            var relativePath = imageUrl.TrimStart('/');
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                return Ok(new { Success = true, Data = true, Message = "Banner image deleted successfully" });
            }

            return NotFound(new { Success = false, Message = "File not found", Errors = new[] { "File not found" } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Delete failed: {ex.Message}", Errors = new[] { "Delete failed" } });
        }
    }

    #endregion

    #region Public Access

    /// <summary>
    /// Lấy banner kích hoạt theo vị trí (Public)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveBannersAsync([FromQuery] string? position = null)
    {
        try
        {
            var result = await _bannerService.GetActiveBannersAsync(position);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Error retrieving active banners: {ex.Message}", Errors = new[] { "Server error" } });
        }
    }

    /// <summary>
    /// Lấy tất cả banner kích hoạt (Public)
    /// </summary>
    [HttpGet("all-active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllActiveBannersAsync()
    {
        try
        {
            var result = await _bannerService.GetActiveBannersAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Error retrieving all active banners: {ex.Message}", Errors = new[] { "Server error" } });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Lấy danh sách các vị trí banner có sẵn
    /// </summary>
    [HttpGet("positions")]
    public IActionResult GetBannerPositions()
    {
        var positions = new List<object>
        {
            new { Value = "homepage", Label = "Trang chủ" },
            new { Value = "category", Label = "Trang danh mục" },
            new { Value = "product", Label = "Trang sản phẩm" },
            new { Value = "sidebar", Label = "Thanh bên" },
            new { Value = "header", Label = "Đầu trang" },
            new { Value = "footer", Label = "Cuối trang" }
        };

        return Ok(new { Success = true, Data = positions, Message = "Banner positions retrieved successfully" });
    }

    #endregion
}