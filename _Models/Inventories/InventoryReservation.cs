namespace TheLightStore.Models.Inventories;

public partial class InventoryReservation
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public int? CartId { get; set; }

    public string? SessionId { get; set; }

    public int? UserId { get; set; }

    public int Quantity { get; set; }

    public DateTime ReservedUntil { get; set; }

    public string Status { get; set; } = null!;

    public int? OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ShoppingCart? Cart { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual GuestSession? Session { get; set; }

    public virtual User? User { get; set; }

    public virtual ProductVariant? Variant { get; set; }
}
