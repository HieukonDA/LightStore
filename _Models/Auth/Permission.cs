namespace TheLightStore.Models.Auth;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // user.create, product.read, order.update, etc.
    public string? Description { get; set; }
    public string Module { get; set; } = null!; // User, Product, Order, Blog, etc.
    public string Action { get; set; } = null!; // Create, Read, Update, Delete, Manage
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
