namespace TheLightStore.Models.ProductReviews;

public class ProductReview
{
   public int Id { get; set; }

    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public int? OrderId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public int Rating { get; set; }

    public string? Title { get; set; }

    public string Comment { get; set; } = null!;

    public string? Images { get; set; }

    public string? Status { get; set; }

    public bool? IsVerifiedPurchase { get; set; }

    public int? HelpfulCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; } = new List<ReviewHelpfulVote>();

    public virtual User? User { get; set; }
}