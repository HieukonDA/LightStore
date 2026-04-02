using System;
using TheLightStore.Domain.Commons.Models;

namespace TheLightStore.Domain.Entities.Employees;

public class Employee : BaseEntity
{
    public string Code { get; set; } = null!;
    public string UserId { get; set; } = null!;
}
