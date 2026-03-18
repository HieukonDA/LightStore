using System;

namespace TheLightStore.Domain.Commons.Models;

public interface IAuditableEntity
{
    bool IsActive { get; set; }
    DateTime CreatedDate { get; set; } 
    string? CreatedBy { get; set; }    
    DateTime? UpdatedDate { get; set; }
    string? UpdatedBy { get; set; }
}

public abstract class BaseEntity<TKey> : IAuditableEntity
{
    public virtual TKey Id { get; set; } = default!;

    // Implements IAuditableEntity
    public bool IsActive { get; set; } = true; // Mặc định là true khi mới tạo
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
}

public abstract class BaseEntity : BaseEntity<Guid>
{
}
