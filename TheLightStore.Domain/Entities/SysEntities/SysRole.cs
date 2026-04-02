using System;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysRole
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public virtual required ICollection<SysUserRole> UserRoles { get; set; }
    public virtual required ICollection<SysRolePermission> RolePermissions { get; set; }
}
