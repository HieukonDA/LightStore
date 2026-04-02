using System;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysUserRole
{
    public required string UserId { get; set; }  // From IdentityUser
    public long RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual required Users User { get; set; }
    public virtual required SysRole Role { get; set; }
}
