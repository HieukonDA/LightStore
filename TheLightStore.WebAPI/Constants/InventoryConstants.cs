namespace TheLightStore.Constants;

public static class InventoryStatus
{
    public const string Active = "active";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
    public const string Expired = "Expired";
}

public static class InventoryChangeType
{
    public const string Reserved = "Reserved";
    public const string Committed = "Committed";
    public const string Released = "Released";
    public const string Expired = "Expired";
    public const string Manual = "Manual";
    public const string Purchase = "Purchase";
    public const string Sale = "Sale";
    public const string Adjustment = "Adjustment";
}

public static class OrderStatus
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Processing = "processing";
    public const string Shipping = "shipping";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";
    public const string Refunded = "refunded";
}