namespace TheLightStore.Models.Product;

public class ProductReview
{
   public int Id { get; set; }
   public int ProductId { get; set; }
   public int UserId { get; set; } = 0;
   public int OrderId { get; set; } = 0;
   public string CustomerName { get; set; } = string.Empty;
   public string CustomerEmail { get; set; } = string.Empty;
   public int Rating { get; set; } = 1;
   public string Title { get; set; } = string.Empty;
   public string Comment { get; set; } = string.Empty;
   public string Images { get; set; } = string.Empty;
   public string Status { get; set; } = "pending";
   public bool IsVerifiedPurchase { get; set; } = false;
   public int HelpfulCount { get; set; } = 0;
   public DateTime CreatedAt { get; set; } = DateTime.Now;
   public DateTime ApprovedAt { get; set; } = DateTime.Now;
}