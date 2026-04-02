using System;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysRolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual required SysRole Role { get; set; }
    public virtual required SysPermission Permission { get; set; }
}
