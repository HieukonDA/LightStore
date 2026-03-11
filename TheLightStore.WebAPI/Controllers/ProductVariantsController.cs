using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.DTOs.Products;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.DTOs;

namespace TheLightStore.Controllers.Products;

[ApiController]
[Route("api/v1/products/{productId}/[controller]")]
public class VariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;
    private readonly ILogger<VariantsController> _logger;

    public VariantsController(IProductVariantService variantService, ILogger<VariantsController> logger)
    {
        _variantService = variantService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách variants của một product (chỉ active)
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <returns>Danh sách variants</returns>
    [HttpGet]
    public async Task<IActionResult> GetByProductIdAsync(int productId)
    {
        var result = await _variantService.GetByProductIdAsync(productId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Lấy tất cả variants của một product (bao gồm inactive)
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <returns>Danh sách tất cả variants</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByProductIdForAdminAsync(int productId)
    {
        var result = await _variantService.GetByProductIdForAdminAsync(productId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Lấy chi tiết một variant (chỉ nếu active)
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Chi tiết variant</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int productId, int id)
    {
        var result = await _variantService.GetByIdAsync(id);
        if (result.Success)
        {
            // Verify variant belongs to the product
            if (result.Data?.ProductId != productId)
            {
                return NotFound(ServiceResult<ProductVariantDto>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
            }
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Lấy chi tiết variant bất kể trạng thái
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Chi tiết variant</returns>
    [HttpGet("{id}/admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByIdForAdminAsync(int productId, int id)
    {
        var result = await _variantService.GetByIdForAdminAsync(id);
        if (result.Success)
        {
            // Verify variant belongs to the product
            if (result.Data?.ProductId != productId)
            {
                return NotFound(ServiceResult<ProductVariantDto>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
            }
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Tạo variant mới cho sản phẩm
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="dto">Thông tin variant mới</param>
    /// <returns>Thông tin variant đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateAsync(int productId, [FromBody] CreateProductVariantDto dto)
    {
        var result = await _variantService.CreateAsync(productId, dto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Cập nhật variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="dto">Thông tin cập nhật</param>
    /// <returns>Thông tin variant đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAsync(int productId, int id, [FromBody] UpdateProductVariantDto dto)
    {
        // 🔥 Verify variant belongs to product (sử dụng admin method để có thể update cả inactive variants)
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<ProductVariantDto>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.UpdateAsync(id, dto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Xóa variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(int productId, int id)
    {
        // 🔥 Verify variant belongs to product (sử dụng admin method để có thể delete cả inactive variants)
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<bool>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.DeleteAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Cập nhật kho hàng cho variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="quantity">Số lượng mới</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStockAsync(int productId, int id, [FromBody] int quantity)
    {
        // 🔥 Verify variant belongs to product
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<bool>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.UpdateStockAsync(id, quantity);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Kiểm tra trạng thái kho hàng
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Thông tin trạng thái kho</returns>
    [HttpGet("{id}/stock-status")]
    public async Task<IActionResult> CheckStockAsync(int productId, int id)
    {
        var result = await _variantService.CheckStockAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Cập nhật thuộc tính cho variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="attributes">Danh sách thuộc tính</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("{id}/attributes")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAttributesAsync(int productId, int id, [FromBody] List<ProductVariantAttributeDto> attributes)
    {
        // 🔥 Verify variant belongs to product
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<bool>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.UpdateAttributesAsync(id, attributes);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Lấy thuộc tính của variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Danh sách thuộc tính</returns>
    [HttpGet("{id}/attributes")]
    public async Task<IActionResult> GetAttributesAsync(int productId, int id)
    {
        var result = await _variantService.GetAttributesAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Toggle trạng thái active/inactive
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Kết quả toggle</returns>
    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ToggleActiveStatusAsync(int productId, int id)
    {
        // 🔥 Verify variant belongs to product (sử dụng admin method)
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<bool>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.ToggleActiveStatusAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Cập nhật thứ tự sắp xếp
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="sortOrder">Thứ tự mới</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPatch("{id}/sort-order")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateSortOrderAsync(int productId, int id, [FromBody] int sortOrder)
    {
        // 🔥 Verify variant belongs to product (sử dụng admin method)
        var existingResult = await _variantService.GetByIdForAdminAsync(id);
        if (!existingResult.Success || existingResult.Data?.ProductId != productId)
        {
            return NotFound(ServiceResult<bool>.FailureResult("Variant not found for this product", new List<string> { "Variant not found" }));
        }

        var result = await _variantService.UpdateSortOrderAsync(id, sortOrder);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Tính giá cuối cùng cho variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <returns>Giá cuối cùng</returns>
    [HttpGet("{id}/final-price")]
    public async Task<IActionResult> CalculateFinalPriceAsync(int productId, int id)
    {
        var result = await _variantService.CalculateFinalPriceAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Sinh SKU tự động
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="attributes">Thuộc tính để sinh SKU</param>
    /// <returns>SKU được sinh tự động</returns>
    [HttpPost("generate-sku")]
    public async Task<IActionResult> GenerateSkuAsync(int productId, [FromBody] List<ProductVariantAttributeDto> attributes)
    {
        var result = await _variantService.GenerateSkuAsync(productId, attributes);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của SKU
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="sku">SKU cần kiểm tra</param>
    /// <param name="excludeVariantId">ID variant cần loại trừ (khi update)</param>
    /// <returns>Kết quả kiểm tra</returns>
    [HttpPost("validate-sku")]
    public async Task<IActionResult> ValidateSkuAsync(int productId, [FromQuery] string sku, [FromQuery] int? excludeVariantId = null)
    {
        var result = await _variantService.ValidateSkuAsync(sku, excludeVariantId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Gán ảnh cho variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="imageId">ID của ảnh</param>
    /// <returns>Kết quả gán ảnh</returns>
    [HttpPost("{id}/images/{imageId}")]
    public async Task<IActionResult> AssignImageAsync(int productId, int id, int imageId)
    {
        var result = await _variantService.AssignImageAsync(id, imageId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Bỏ gán ảnh khỏi variant
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="id">ID của variant</param>
    /// <param name="imageId">ID của ảnh</param>
    /// <returns>Kết quả bỏ gán</returns>
    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> RemoveImageAsync(int productId, int id, int imageId)
    {
        var result = await _variantService.RemoveImageAsync(id, imageId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }
}

/// <summary>
/// Controller để quản lý variants theo cách khác (có thể truy cập trực tiếp bằng variant ID)
/// </summary>
[ApiController]
[Route("api/v1/variants")]
public class ProductVariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;
    private readonly ILogger<ProductVariantsController> _logger;

    public ProductVariantsController(IProductVariantService variantService, ILogger<ProductVariantsController> logger)
    {
        _variantService = variantService;
        _logger = logger;
    }

    /// <summary>
    /// Tìm variant theo SKU
    /// </summary>
    /// <param name="sku">SKU của variant</param>
    /// <returns>Chi tiết variant</returns>
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySkuAsync(string sku)
    {
        var result = await _variantService.GetBySkuAsync(sku);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Lấy danh sách variants có tồn kho thấp
    /// </summary>
    /// <param name="productId">ID sản phẩm (optional)</param>
    /// <returns>Danh sách variants tồn kho thấp</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockVariantsAsync([FromQuery] int? productId = null)
    {
        var result = await _variantService.GetLowStockVariantsAsync(productId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Lấy danh sách variants hết hàng
    /// </summary>
    /// <param name="productId">ID sản phẩm (optional)</param>
    /// <returns>Danh sách variants hết hàng</returns>
    [HttpGet("out-of-stock")]
    public async Task<IActionResult> GetOutOfStockVariantsAsync([FromQuery] int? productId = null)
    {
        var result = await _variantService.GetOutOfStockVariantsAsync(productId);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Lấy chi tiết variant trực tiếp (không cần thông qua product) - chỉ active
    /// </summary>
    /// <param name="id">ID của variant</param>
    /// <returns>Chi tiết variant</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdDirectAsync(int id)
    {
        var result = await _variantService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// 🔥 Admin - Lấy chi tiết variant trực tiếp bất kể trạng thái
    /// </summary>
    /// <param name="id">ID của variant</param>
    /// <returns>Chi tiết variant</returns>
    [HttpGet("{id}/admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByIdDirectForAdminAsync(int id)
    {
        var result = await _variantService.GetByIdForAdminAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }
}