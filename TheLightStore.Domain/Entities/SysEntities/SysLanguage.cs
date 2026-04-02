using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysLanguage : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool? IsDefault { get; set; }
    public string? FlagPath { get; set; }
    public Guid? ImageId { get; set; }
    public virtual SysFile Image { get; set; } = null!;
}
