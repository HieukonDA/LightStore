using System;
using Microsoft.AspNetCore.Identity;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class Roles : IdentityRole
{
    public string? JsonRoleHasFunctions { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public bool? IsActive { get; set; }
}
