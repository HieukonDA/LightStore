using Microsoft.EntityFrameworkCore;
using TheLightStore.Datas;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.Blogs;

namespace TheLightStore.Repositories;

public class BlogRepository : IBlogRepository
{
    private readonly DBContext _context;

    public BlogRepository(DBContext context)
    {
        _context = context;
    }

    #region Blog Posts

    public async Task<IEnumerable<BlogPost>> GetBlogPostsAsync(int pageNumber, int pageSize, string? status = null, int? categoryId = null, string? search = null)
    {
        var query = _context.BlogPosts
            .Include(bp => bp.BlogCategory)
            .Include(bp => bp.Author)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(bp => bp.Status == status);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(bp => bp.BlogCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(bp => bp.Title.Contains(search) || 
                                    (bp.Excerpt != null && bp.Excerpt.Contains(search)) ||
                                    bp.Content.Contains(search));
        }

        return await query
            .OrderByDescending(bp => bp.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetBlogPostsCountAsync(string? status = null, int? categoryId = null, string? search = null)
    {
        var query = _context.BlogPosts.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(bp => bp.Status == status);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(bp => bp.BlogCategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(bp => bp.Title.Contains(search) || 
                                    (bp.Excerpt != null && bp.Excerpt.Contains(search)) ||
                                    bp.Content.Contains(search));
        }

        return await query.CountAsync();
    }

    public async Task<BlogPost?> GetBlogPostByIdAsync(int id)
    {
        return await _context.BlogPosts
            .Include(bp => bp.BlogCategory)
            .Include(bp => bp.Author)
            .FirstOrDefaultAsync(bp => bp.Id == id);
    }

    public async Task<BlogPost?> GetBlogPostBySlugAsync(string slug)
    {
        return await _context.BlogPosts
            .Include(bp => bp.BlogCategory)
            .Include(bp => bp.Author)
            .FirstOrDefaultAsync(bp => bp.Slug == slug);
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
    {
        blogPost.CreatedAt = DateTime.UtcNow;
        
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();
        
        return await GetBlogPostByIdAsync(blogPost.Id) ?? blogPost;
    }

    public async Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost)
    {
        blogPost.UpdatedAt = DateTime.UtcNow;
        
        _context.BlogPosts.Update(blogPost);
        await _context.SaveChangesAsync();
        
        return await GetBlogPostByIdAsync(blogPost.Id) ?? blogPost;
    }

    public async Task<bool> DeleteBlogPostAsync(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null) return false;

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BlogPostExistsBySlugAsync(string slug, int? excludeId = null)
    {
        var query = _context.BlogPosts.Where(bp => bp.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(bp => bp.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region Blog Categories

    public async Task<IEnumerable<BlogCategory>> GetBlogCategoriesAsync(bool activeOnly = false)
    {
        var query = _context.BlogCategories.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(bc => bc.IsActive == true);
        }

        return await query
            .OrderBy(bc => bc.Name)
            .ToListAsync();
    }

    public async Task<BlogCategory?> GetBlogCategoryByIdAsync(int id)
    {
        return await _context.BlogCategories.FindAsync(id);
    }

    public async Task<BlogCategory> CreateBlogCategoryAsync(BlogCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        
        _context.BlogCategories.Add(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<BlogCategory> UpdateBlogCategoryAsync(BlogCategory category)
    {
        _context.BlogCategories.Update(category);
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<bool> DeleteBlogCategoryAsync(int id)
    {
        var category = await _context.BlogCategories.FindAsync(id);
        if (category == null) return false;

        // Check if category has blog posts
        var hasPost = await _context.BlogPosts.AnyAsync(bp => bp.BlogCategoryId == id);
        if (hasPost) return false;

        _context.BlogCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BlogCategoryExistsBySlugAsync(string slug, int? excludeId = null)
    {
        var query = _context.BlogCategories.Where(bc => bc.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(bc => bc.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetPostsCountByCategoryAsync(int categoryId)
    {
        return await _context.BlogPosts.CountAsync(bp => bp.BlogCategoryId == categoryId);
    }

    #endregion
}