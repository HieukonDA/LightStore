using TheLightStore.Dtos;
using TheLightStore.Dtos.Blog;
using TheLightStore.Dtos.Paging;

namespace TheLightStore.Interfaces.Blog;

public interface IBlogService
{
    // Blog Posts
    Task<ApiResponse<PagedResult<BlogPostListDto>>> GetBlogPostsAsync(int pageNumber = 1, int pageSize = 10, string? status = null, int? categoryId = null, string? search = null);
    Task<ApiResponse<BlogPostDto>> GetBlogPostByIdAsync(int id);
    Task<ApiResponse<BlogPostDto>> GetBlogPostBySlugAsync(string slug);
    Task<ApiResponse<BlogPostDto>> CreateBlogPostAsync(CreateBlogPostDto createDto, int authorId);
    Task<ApiResponse<BlogPostDto>> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateDto);
    Task<ApiResponse<bool>> DeleteBlogPostAsync(int id);
    Task<ApiResponse<bool>> PublishBlogPostAsync(int id);
    Task<ApiResponse<bool>> ArchiveBlogPostAsync(int id);
    
    // Blog Categories
    Task<ApiResponse<List<BlogCategoryDto>>> GetBlogCategoriesAsync(bool activeOnly = false);
    Task<ApiResponse<BlogCategoryDto>> GetBlogCategoryByIdAsync(int id);
    Task<ApiResponse<BlogCategoryDto>> CreateBlogCategoryAsync(CreateBlogCategoryDto createDto);
    Task<ApiResponse<BlogCategoryDto>> UpdateBlogCategoryAsync(int id, UpdateBlogCategoryDto updateDto);
    Task<ApiResponse<bool>> DeleteBlogCategoryAsync(int id);
    
    // Public methods for frontend
    Task<ApiResponse<PagedResult<BlogPostListDto>>> GetPublishedBlogPostsAsync(int pageNumber = 1, int pageSize = 10, int? categoryId = null, string? search = null);
    Task<ApiResponse<List<BlogPostListDto>>> GetFeaturedBlogPostsAsync(int take = 5);
}