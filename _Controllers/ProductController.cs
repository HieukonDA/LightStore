namespace TheLightStore.Controllers.Products;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
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
            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Data.Id }, result);
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

    #endregion
}