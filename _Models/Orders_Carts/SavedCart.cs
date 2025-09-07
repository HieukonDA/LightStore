namespace TheLightStore.Models.Orders_Carts;

public partial class SavedCart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? CartName { get; set; }

    public string CartData { get; set; } = null!;

    public int? ItemsCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
