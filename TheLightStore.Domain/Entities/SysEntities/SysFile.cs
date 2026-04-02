using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysFile : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Type { get; set; } = null!;
}
