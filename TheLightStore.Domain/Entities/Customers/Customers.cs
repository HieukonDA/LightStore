
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Customers;

public class Customer : BaseEntity
{
    public string IdentityUserId { get; set; } = null!;
    public string Code { get; set; } = null!;
    public long? CustomerTypeId { get; set; }
    public long? TotalCancelOrder { get; set; }
    public long? TotalCompletedOrder { get; set; }
    public long? TotalBuyingProduct { get; set; }
    public long? TotalPoints { get; set; }
    public decimal? TotalPayment { get; set; }
    public string? Description { get; set; }
    public virtual CustomerType CustomerType { get; set; } = null!;
}
