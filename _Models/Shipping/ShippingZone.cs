namespace TheLightStore.Models.Shipping;

public partial class ShippingZone
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Provinces { get; set; } = null!;

    public decimal ShippingCost { get; set; }

    public decimal? FreeShippingThreshold { get; set; }

    public string? EstimatedDeliveryDays { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }
}
