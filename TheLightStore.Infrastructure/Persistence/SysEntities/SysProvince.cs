using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public partial class SysProvince : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<SysWard> SysWards { get; set; } = [];
}
