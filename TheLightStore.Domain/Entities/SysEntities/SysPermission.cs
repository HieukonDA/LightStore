using System;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysPermission
{
    public long Id { get; set; }
    public required string Name { get; set; }           // user.create, product.read
    public required string Module { get; set; }         // User, Product, Order
    public required string Action { get; set; }         // Create, Read, Update, Delete
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual required ICollection<SysRolePermission> RolePermissions { get; set; }
}
