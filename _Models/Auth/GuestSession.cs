

namespace TheLightStore.Models.Auth;

public partial class GuestSession
{
    public string Id { get; set; } = null!;

    public string? GuestEmail { get; set; }

    public string? GuestPhone { get; set; }

    public string? GuestName { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? LastActivity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
}
