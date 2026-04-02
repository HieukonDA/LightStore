using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class Users : IdentityUser
{
    public string? DomainUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool? IsCustomer { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsMobile { get; set; }
    public bool? Sex { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public virtual SysRole? Role { get; set; }
}
