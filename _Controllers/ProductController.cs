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

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImageAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ServiceResult<string>.FailureResult("No file uploaded", new List<string> { "File is required" }));
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

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Create directory structure: uploads/products/YYYY/MM/
            var currentDate = DateTime.Now;
            var relativePath = Path.Combine("uploads", "products", 
                                        currentDate.Year.ToString(), 
                                        currentDate.Month.ToString("D2"));
            
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            
            // Ensure directory exists
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, fileName);
            var relativeFilePath = Path.Combine(relativePath, fileName).Replace("\\", "/");

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative URL
            var imageUrl = $"/{relativeFilePath}";
            
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