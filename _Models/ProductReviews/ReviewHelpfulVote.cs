namespace TheLightStore.Models.ProductReviews;

public partial class ReviewHelpfulVote
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public string? IpAddress { get; set; }

    public bool IsHelpful { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ProductReview Review { get; set; } = null!;

    public virtual User? User { get; set; }
}
