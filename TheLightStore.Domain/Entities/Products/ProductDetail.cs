using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Products;

public class ProductDetail : BaseEntity<long>
{
    // Foreign Keys
    public long ProductId { get; set; }
    public long? PowerId { get; set; }
    public long? ColorTemperatureId { get; set; }
    public long? ShapeId { get; set; }
    public long? BaseTypeId { get; set; }

    // Properties
    public decimal SellingPrice { get; set; }
    public long EarningPoints { get; set; }
    public long? SoldQuantity { get; set; }
    public string? Description { get; set; }
    public long? CurrencyId { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Power? Power { get; set; }
    public virtual ColorTemperature? ColorTemperature { get; set; }
    public virtual Shape? Shape { get; set; }
    public virtual BaseType? BaseType { get; set; }
}
