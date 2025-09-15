namespace TheLightStore.Models.Auth;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // Admin, Manager, Customer, Staff, etc.
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}