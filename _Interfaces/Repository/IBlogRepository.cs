using TheLightStore.Models.Blogs;

namespace TheLightStore.Interfaces.Repository;

public interface IBlogRepository
{
    // Blog Posts
    Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int pageNumber, int pageSize, string? status = null, int? categoryId = null, string? search = null);
    Task<int> GetBlogPostsCountAsync(string? status = null, int? categoryId = null, string? search = null);
    Task<BlogPost?> GetBlogPostByIdAsync(int id);
    Task<BlogPost?> GetBlogPostBySlugAsync(string slug);
    Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
    Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost);
    Task<bool> DeleteBlogPostAsync(int id);
    Task<bool> BlogPostExistsBySlugAsync(string slug, int? excludeId = null);
    
    // Blog Categories
    Task<IEnumerable<BlogCategory>> GetBlogCategoriesAsync(bool activeOnly = false);
    Task<BlogCategory?> GetBlogCategoryByIdAsync(int id);
    Task<BlogCategory> CreateBlogCategoryAsync(BlogCategory category);
    Task<BlogCategory> UpdateBlogCategoryAsync(BlogCategory category);
    Task<bool> DeleteBlogCategoryAsync(int id);
    Task<bool> BlogCategoryExistsBySlugAsync(string slug, int? excludeId = null);
    Task<int> GetPostsCountByCategoryAsync(int categoryId);
}