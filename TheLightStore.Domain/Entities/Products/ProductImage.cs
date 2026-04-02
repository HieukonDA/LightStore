using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Infrastructure.Persistence.SysEntities;

namespace TheLightStore.Domain.Entities.Products;

public class ProductImage : BaseEntity<long>
{
    public long ProductId { get; set; }
    [ForeignKey(nameof(SysFile))]
    public Guid FileId { get; set; }

    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
    public virtual SysFile SysFile { get; set; } = null!;
}
