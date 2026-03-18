using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public partial class SysWard : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public long ProvinceId { get; set; }
    public virtual SysProvince Province { get; set; } = null!;
}
