namespace TheLightStore.Models.Inventories;
public partial class InventoryLog
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public string ChangeType { get; set; } = null!;

    public int QuantityBefore { get; set; }

    public int QuantityChange { get; set; }

    public int QuantityAfter { get; set; }

    public string? Reason { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariant? Variant { get; set; }
}
