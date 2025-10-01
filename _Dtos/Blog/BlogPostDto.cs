namespace TheLightStore.Dtos.Blog;

public class BlogPostDto
{
    public int Id { get; set; }
    public int? BlogCategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = null!;
    public string? FeaturedImage { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string Status { get; set; } = "draft"; // draft, published, archived
    public bool IsFeatured { get; set; } = false;
    public int AuthorId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related data
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
}

public class CreateBlogPostDto
{
    public int? BlogCategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = null!;
    public string? FeaturedImage { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string Status { get; set; } = "draft";
    public bool IsFeatured { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
}

public class UpdateBlogPostDto
{
    public int? BlogCategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = null!;
    public string? FeaturedImage { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string Status { get; set; } = "draft";
    public bool IsFeatured { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
}

public class BlogPostListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? FeaturedImage { get; set; }
    public string Status { get; set; } = null!;
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public string AuthorName { get; set; } = null!;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}