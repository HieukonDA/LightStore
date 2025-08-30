
namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Define your action methods here

    [HttpGet]
    public async Task<IActionResult> GetAllCategories([FromQuery] PagedRequest pagedRequest)
    {
        var result = await _categoryService.GetAllCategoriesAsync(pagedRequest);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result.Errors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        if (result.Success)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Errors);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryDto)
    {
        var result = await _categoryService.CreateCategoryAsync(categoryDto);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data.Id }, result.Data);
        }
        return BadRequest(result.Errors);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);
        if (result.Success)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (result.Success)
        {
            return NoContent();
        }
        return BadRequest(result.Errors);
    }

}