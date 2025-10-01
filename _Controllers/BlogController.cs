using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Dtos.Blog;
using TheLightStore.Interfaces.Blog;

namespace TheLightStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    #region Public Methods (No Authentication Required)

    /// <summary>
    /// Lấy danh sách bài viết đã xuất bản cho công chúng
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublishedBlogPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        var response = await _blogService.GetPublishedBlogPostsAsync(pageNumber, pageSize, categoryId, search);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy bài viết theo slug
    /// </summary>
    [HttpGet("public/{slug}")]
    public async Task<IActionResult> GetBlogPostBySlug(string slug)
    {
        var response = await _blogService.GetBlogPostBySlugAsync(slug);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy danh sách bài viết nổi bật
    /// </summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedBlogPosts([FromQuery] int take = 5)
    {
        var response = await _blogService.GetFeaturedBlogPostsAsync(take);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy danh sách danh mục blog (chỉ active)
    /// </summary>
    [HttpGet("categories/public")]
    public async Task<IActionResult> GetActiveBlogCategories()
    {
        var response = await _blogService.GetBlogCategoriesAsync(activeOnly: true);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    #endregion

    #region Admin/Manager Methods (Authentication Required)

    /// <summary>
    /// Lấy danh sách tất cả bài viết (Admin/Manager)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBlogPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        var response = await _blogService.GetBlogPostsAsync(pageNumber, pageSize, status, categoryId, search);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy bài viết theo ID (Admin/Manager)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBlogPostById(int id)
    {
        var response = await _blogService.GetBlogPostByIdAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Tạo bài viết mới (Admin/Manager)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int authorId))
        {
            return Unauthorized(new { Success = false, Message = "Invalid user" });
        }

        var response = await _blogService.CreateBlogPostAsync(createDto, authorId);
        
        if (response.Success)
            return CreatedAtAction(nameof(GetBlogPostById), new { id = response.Data!.Id }, response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Cập nhật bài viết (Admin/Manager)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] UpdateBlogPostDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var response = await _blogService.UpdateBlogPostAsync(id, updateDto);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Xóa bài viết (Admin/Manager)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteBlogPost(int id)
    {
        var response = await _blogService.DeleteBlogPostAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Xuất bản bài viết (Admin/Manager)
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PublishBlogPost(int id)
    {
        var response = await _blogService.PublishBlogPostAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lưu trữ bài viết (Admin/Manager)
    /// </summary>
    [HttpPost("{id}/archive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ArchiveBlogPost(int id)
    {
        var response = await _blogService.ArchiveBlogPostAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    #endregion

    #region Blog Categories Management

    /// <summary>
    /// Lấy danh sách danh mục blog (Admin/Manager)
    /// </summary>
    [HttpGet("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBlogCategories([FromQuery] bool activeOnly = false)
    {
        var response = await _blogService.GetBlogCategoriesAsync(activeOnly);
        
        if (response.Success)
            return Ok(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Lấy danh mục blog theo ID (Admin/Manager)
    /// </summary>
    [HttpGet("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetBlogCategoryById(int id)
    {
        var response = await _blogService.GetBlogCategoryByIdAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Tạo danh mục blog mới (Admin/Manager)
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateBlogCategory([FromBody] CreateBlogCategoryDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var response = await _blogService.CreateBlogCategoryAsync(createDto);
        
        if (response.Success)
            return CreatedAtAction(nameof(GetBlogCategoryById), new { id = response.Data!.Id }, response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Cập nhật danh mục blog (Admin/Manager)
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateBlogCategory(int id, [FromBody] UpdateBlogCategoryDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });
        }

        var response = await _blogService.UpdateBlogCategoryAsync(id, updateDto);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found"))
            return NotFound(response);
            
        return BadRequest(response);
    }

    /// <summary>
    /// Xóa danh mục blog (Admin/Manager)
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteBlogCategory(int id)
    {
        var response = await _blogService.DeleteBlogCategoryAsync(id);
        
        if (response.Success)
            return Ok(response);
            
        if (response.Message.Contains("not found") || response.Message.Contains("associated"))
            return BadRequest(response);
            
        return BadRequest(response);
    }

    #endregion
}