using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Customers;

public class CustomerType : BaseEntity<long>
{
    public string Name { get; set; } = null!;
    public long Points { get; set; }
    public string? Description { get; set; }
    public int PercentDiscount { get; set; }
    public string? Code { get; set; }
}
