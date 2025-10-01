using System.Text.RegularExpressions;
using TheLightStore.Dtos;
using TheLightStore.Dtos.Blog;
using TheLightStore.Dtos.Paging;
using TheLightStore.Interfaces.Blog;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Models.Blogs;

namespace TheLightStore.Services.cs;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _blogRepository;

    public BlogService(IBlogRepository blogRepository)
    {
        _blogRepository = blogRepository;
    }

    #region Blog Posts

    public async Task<ApiResponse<PagedResult<BlogPostListDto>>> GetBlogPostsAsync(int pageNumber = 1, int pageSize = 10, string? status = null, int? categoryId = null, string? search = null)
    {
        try
        {
            var blogPosts = await _blogRepository.GetBlogPostsAsync(pageNumber, pageSize, status, categoryId, search);
            var totalCount = await _blogRepository.GetBlogPostsCountAsync(status, categoryId, search);

            var blogPostDtos = blogPosts.Select(bp => new BlogPostListDto
            {
                Id = bp.Id,
                Title = bp.Title,
                Slug = bp.Slug,
                Excerpt = bp.Excerpt,
                FeaturedImage = bp.FeaturedImage,
                Status = bp.Status,
                IsFeatured = bp.IsFeatured ?? false,
                CategoryName = bp.BlogCategory?.Name,
                AuthorName = $"{bp.Author.FirstName} {bp.Author.LastName}".Trim(),
                PublishedAt = bp.PublishedAt,
                CreatedAt = bp.CreatedAt ?? DateTime.UtcNow
            }).ToList();

            var pagedResult = new PagedResult<BlogPostListDto>
            {
                Items = blogPostDtos,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<BlogPostListDto>>
            {
                Success = true,
                Data = pagedResult,
                Message = "Blog posts retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<BlogPostListDto>>
            {
                Success = false,
                Message = $"Error retrieving blog posts: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogPostDto>> GetBlogPostByIdAsync(int id)
    {
        try
        {
            var blogPost = await _blogRepository.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return new ApiResponse<BlogPostDto>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            var blogPostDto = new BlogPostDto
            {
                Id = blogPost.Id,
                BlogCategoryId = blogPost.BlogCategoryId,
                Title = blogPost.Title,
                Slug = blogPost.Slug,
                Excerpt = blogPost.Excerpt,
                Content = blogPost.Content,
                FeaturedImage = blogPost.FeaturedImage,
                MetaTitle = blogPost.MetaTitle,
                MetaDescription = blogPost.MetaDescription,
                Status = blogPost.Status,
                IsFeatured = blogPost.IsFeatured ?? false,
                AuthorId = blogPost.AuthorId,
                PublishedAt = blogPost.PublishedAt,
                CreatedAt = blogPost.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = blogPost.UpdatedAt,
                CategoryName = blogPost.BlogCategory?.Name,
                AuthorName = $"{blogPost.Author.FirstName} {blogPost.Author.LastName}".Trim()
            };

            return new ApiResponse<BlogPostDto>
            {
                Success = true,
                Data = blogPostDto,
                Message = "Blog post retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogPostDto>
            {
                Success = false,
                Message = $"Error retrieving blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogPostDto>> GetBlogPostBySlugAsync(string slug)
    {
        try
        {
            var blogPost = await _blogRepository.GetBlogPostBySlugAsync(slug);
            if (blogPost == null)
            {
                return new ApiResponse<BlogPostDto>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            var blogPostDto = new BlogPostDto
            {
                Id = blogPost.Id,
                BlogCategoryId = blogPost.BlogCategoryId,
                Title = blogPost.Title,
                Slug = blogPost.Slug,
                Excerpt = blogPost.Excerpt,
                Content = blogPost.Content,
                FeaturedImage = blogPost.FeaturedImage,
                MetaTitle = blogPost.MetaTitle,
                MetaDescription = blogPost.MetaDescription,
                Status = blogPost.Status,
                IsFeatured = blogPost.IsFeatured ?? false,
                AuthorId = blogPost.AuthorId,
                PublishedAt = blogPost.PublishedAt,
                CreatedAt = blogPost.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = blogPost.UpdatedAt,
                CategoryName = blogPost.BlogCategory?.Name,
                AuthorName = $"{blogPost.Author.FirstName} {blogPost.Author.LastName}".Trim()
            };

            return new ApiResponse<BlogPostDto>
            {
                Success = true,
                Data = blogPostDto,
                Message = "Blog post retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogPostDto>
            {
                Success = false,
                Message = $"Error retrieving blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogPostDto>> CreateBlogPostAsync(CreateBlogPostDto createDto, int authorId)
    {
        try
        {
            // Generate slug from title
            var slug = GenerateSlug(createDto.Title);
            
            // Check if slug exists
            var slugExists = await _blogRepository.BlogPostExistsBySlugAsync(slug);
            if (slugExists)
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var blogPost = new BlogPost
            {
                BlogCategoryId = createDto.BlogCategoryId,
                Title = createDto.Title,
                Slug = slug,
                Excerpt = createDto.Excerpt,
                Content = createDto.Content,
                FeaturedImage = createDto.FeaturedImage,
                MetaTitle = createDto.MetaTitle ?? createDto.Title,
                MetaDescription = createDto.MetaDescription ?? createDto.Excerpt,
                Status = createDto.Status,
                IsFeatured = createDto.IsFeatured,
                AuthorId = authorId,
                PublishedAt = createDto.Status == "published" ? (createDto.PublishedAt ?? DateTime.UtcNow) : createDto.PublishedAt
            };

            var createdBlogPost = await _blogRepository.CreateBlogPostAsync(blogPost);
            
            return await GetBlogPostByIdAsync(createdBlogPost.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogPostDto>
            {
                Success = false,
                Message = $"Error creating blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogPostDto>> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateDto)
    {
        try
        {
            var existingBlogPost = await _blogRepository.GetBlogPostByIdAsync(id);
            if (existingBlogPost == null)
            {
                return new ApiResponse<BlogPostDto>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            // Generate new slug if title changed
            var slug = existingBlogPost.Slug;
            if (existingBlogPost.Title != updateDto.Title)
            {
                slug = GenerateSlug(updateDto.Title);
                var slugExists = await _blogRepository.BlogPostExistsBySlugAsync(slug, id);
                if (slugExists)
                {
                    slug = $"{slug}-{DateTime.UtcNow.Ticks}";
                }
            }

            existingBlogPost.BlogCategoryId = updateDto.BlogCategoryId;
            existingBlogPost.Title = updateDto.Title;
            existingBlogPost.Slug = slug;
            existingBlogPost.Excerpt = updateDto.Excerpt;
            existingBlogPost.Content = updateDto.Content;
            existingBlogPost.FeaturedImage = updateDto.FeaturedImage;
            existingBlogPost.MetaTitle = updateDto.MetaTitle ?? updateDto.Title;
            existingBlogPost.MetaDescription = updateDto.MetaDescription ?? updateDto.Excerpt;
            existingBlogPost.Status = updateDto.Status;
            existingBlogPost.IsFeatured = updateDto.IsFeatured;
            existingBlogPost.PublishedAt = updateDto.Status == "published" && existingBlogPost.PublishedAt == null 
                ? (updateDto.PublishedAt ?? DateTime.UtcNow) 
                : updateDto.PublishedAt;

            var updatedBlogPost = await _blogRepository.UpdateBlogPostAsync(existingBlogPost);
            
            return await GetBlogPostByIdAsync(updatedBlogPost.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogPostDto>
            {
                Success = false,
                Message = $"Error updating blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteBlogPostAsync(int id)
    {
        try
        {
            var deleted = await _blogRepository.DeleteBlogPostAsync(id);
            if (!deleted)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Blog post deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> PublishBlogPostAsync(int id)
    {
        try
        {
            var blogPost = await _blogRepository.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            blogPost.Status = "published";
            blogPost.PublishedAt = DateTime.UtcNow;
            await _blogRepository.UpdateBlogPostAsync(blogPost);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Blog post published successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error publishing blog post: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> ArchiveBlogPostAsync(int id)
    {
        try
        {
            var blogPost = await _blogRepository.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Blog post not found"
                };
            }

            blogPost.Status = "archived";
            await _blogRepository.UpdateBlogPostAsync(blogPost);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Blog post archived successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error archiving blog post: {ex.Message}"
            };
        }
    }

    #endregion

    #region Blog Categories

    public async Task<ApiResponse<List<BlogCategoryDto>>> GetBlogCategoriesAsync(bool activeOnly = false)
    {
        try
        {
            var categories = await _blogRepository.GetBlogCategoriesAsync(activeOnly);
            
            var categoryDtos = new List<BlogCategoryDto>();
            foreach (var category in categories)
            {
                var postsCount = await _blogRepository.GetPostsCountByCategoryAsync(category.Id);
                categoryDtos.Add(new BlogCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description,
                    IsActive = category.IsActive ?? true,
                    CreatedAt = category.CreatedAt ?? DateTime.UtcNow,
                    PostsCount = postsCount
                });
            }

            return new ApiResponse<List<BlogCategoryDto>>
            {
                Success = true,
                Data = categoryDtos,
                Message = "Blog categories retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<BlogCategoryDto>>
            {
                Success = false,
                Message = $"Error retrieving blog categories: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> GetBlogCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _blogRepository.GetBlogCategoryByIdAsync(id);
            if (category == null)
            {
                return new ApiResponse<BlogCategoryDto>
                {
                    Success = false,
                    Message = "Blog category not found"
                };
            }

            var postsCount = await _blogRepository.GetPostsCountByCategoryAsync(id);
            
            var categoryDto = new BlogCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                IsActive = category.IsActive ?? true,
                CreatedAt = category.CreatedAt ?? DateTime.UtcNow,
                PostsCount = postsCount
            };

            return new ApiResponse<BlogCategoryDto>
            {
                Success = true,
                Data = categoryDto,
                Message = "Blog category retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogCategoryDto>
            {
                Success = false,
                Message = $"Error retrieving blog category: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> CreateBlogCategoryAsync(CreateBlogCategoryDto createDto)
    {
        try
        {
            var slug = GenerateSlug(createDto.Name);
            
            var slugExists = await _blogRepository.BlogCategoryExistsBySlugAsync(slug);
            if (slugExists)
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var category = new BlogCategory
            {
                Name = createDto.Name,
                Slug = slug,
                Description = createDto.Description,
                IsActive = createDto.IsActive
            };

            var createdCategory = await _blogRepository.CreateBlogCategoryAsync(category);
            
            return await GetBlogCategoryByIdAsync(createdCategory.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogCategoryDto>
            {
                Success = false,
                Message = $"Error creating blog category: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<BlogCategoryDto>> UpdateBlogCategoryAsync(int id, UpdateBlogCategoryDto updateDto)
    {
        try
        {
            var existingCategory = await _blogRepository.GetBlogCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                return new ApiResponse<BlogCategoryDto>
                {
                    Success = false,
                    Message = "Blog category not found"
                };
            }

            var slug = existingCategory.Slug;
            if (existingCategory.Name != updateDto.Name)
            {
                slug = GenerateSlug(updateDto.Name);
                var slugExists = await _blogRepository.BlogCategoryExistsBySlugAsync(slug, id);
                if (slugExists)
                {
                    slug = $"{slug}-{DateTime.UtcNow.Ticks}";
                }
            }

            existingCategory.Name = updateDto.Name;
            existingCategory.Slug = slug;
            existingCategory.Description = updateDto.Description;
            existingCategory.IsActive = updateDto.IsActive;

            var updatedCategory = await _blogRepository.UpdateBlogCategoryAsync(existingCategory);
            
            return await GetBlogCategoryByIdAsync(updatedCategory.Id);
        }
        catch (Exception ex)
        {
            return new ApiResponse<BlogCategoryDto>
            {
                Success = false,
                Message = $"Error updating blog category: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteBlogCategoryAsync(int id)
    {
        try
        {
            var deleted = await _blogRepository.DeleteBlogCategoryAsync(id);
            if (!deleted)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Blog category not found or has associated blog posts"
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Blog category deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting blog category: {ex.Message}"
            };
        }
    }

    #endregion

    #region Public Methods

    public async Task<ApiResponse<PagedResult<BlogPostListDto>>> GetPublishedBlogPostsAsync(int pageNumber = 1, int pageSize = 10, int? categoryId = null, string? search = null)
    {
        return await GetBlogPostsAsync(pageNumber, pageSize, "published", categoryId, search);
    }

    public async Task<ApiResponse<List<BlogPostListDto>>> GetFeaturedBlogPostsAsync(int take = 5)
    {
        try
        {
            var featuredPosts = await _blogRepository.GetBlogPostsAsync(1, take, "published");
            
            var featuredPostDtos = featuredPosts
                .Where(bp => bp.IsFeatured == true)
                .Select(bp => new BlogPostListDto
                {
                    Id = bp.Id,
                    Title = bp.Title,
                    Slug = bp.Slug,
                    Excerpt = bp.Excerpt,
                    FeaturedImage = bp.FeaturedImage,
                    Status = bp.Status,
                    IsFeatured = bp.IsFeatured ?? false,
                    CategoryName = bp.BlogCategory?.Name,
                    AuthorName = $"{bp.Author.FirstName} {bp.Author.LastName}".Trim(),
                    PublishedAt = bp.PublishedAt,
                    CreatedAt = bp.CreatedAt ?? DateTime.UtcNow
                })
                .Take(take)
                .ToList();

            return new ApiResponse<List<BlogPostListDto>>
            {
                Success = true,
                Data = featuredPostDtos,
                Message = "Featured blog posts retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<BlogPostListDto>>
            {
                Success = false,
                Message = $"Error retrieving featured blog posts: {ex.Message}"
            };
        }
    }

    #endregion

    #region Helper Methods

    private static string GenerateSlug(string title)
    {
        if (string.IsNullOrEmpty(title))
            return string.Empty;

        // Convert to lowercase
        string slug = title.ToLowerInvariant();

        // Remove special characters except spaces and hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    #endregion
}