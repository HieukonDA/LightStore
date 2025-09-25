namespace TheLightStore.Controllers.Products;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
    {
        _productService = productService;
        _webHostEnvironment = webHostEnvironment;
    }

    #region crud basic

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] PagedRequest pagedRequest)
    {
        var result = await _productService.GetPagedAsync(pagedRequest);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
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
        var result = await _productService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }


    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlugAsync(string slug)
    {
        var result = await _productService.GetBySlugAsync(slug);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateAsync(id, dto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _productService.DeleteAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        return BadRequest(result.Errors);
    }

    [HttpPost("upload-image/{productId}")]
    public async Task<IActionResult> UploadImageAsync(int productId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
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
            
            return Ok(ServiceResult<string>.SuccessResult(imageUrl));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ServiceResult<string>.FailureResult($"Upload failed: {ex.Message}", new List<string> { "Upload failed" }));
        }
    }

    [HttpDelete("delete-image")]
    public async Task<IActionResult> DeleteImageAsync([FromQuery] string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(ServiceResult<bool>.FailureResult("Image URL is required", new List<string> { "Image URL is required" }));
            }

            // Remove leading slash if present
            var relativePath = imageUrl.TrimStart('/');
            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
                return Ok(ServiceResult<bool>.SuccessResult(true));
            }

            return NotFound(ServiceResult<bool>.FailureResult("File not found", new List<string> { "File not found" }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ServiceResult<bool>.FailureResult($"Delete failed: {ex.Message}", new List<string> { "Delete failed" }));
        }
    }

    #endregion
}