using System;

namespace TheLightStore.Infrastructure.Persistence.SysEntities;

public class SysApi
{
    public required string ControllerName { get; set; }
    public required string ActionName { get; set; }
    public required string HttpMethod { get; set; }
}
