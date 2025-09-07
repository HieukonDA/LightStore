namespace TheLightStore.Models.Blogs;

public partial class BlogPost
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

    public string Status { get; set; } = null!;

    public bool? IsFeatured { get; set; }

    public int AuthorId { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual BlogCategory? BlogCategory { get; set; }
}
