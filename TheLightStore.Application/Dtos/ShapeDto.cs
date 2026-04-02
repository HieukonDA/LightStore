using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class ShapeDto
{
    public class ShapeCreateDto
    {
        [Required]
        public required string Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ShapeUpdateDto : ShapeCreateDto
    {
        public long Id { get; set; }
    }

    public class ShapeGetDto : ShapeUpdateDto
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ShapeFilterParams
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class ShapeExportDto
    {
        public long Id { get; set; }
        [Display(Name = "Mã kiểu dáng")]
        public string Code { get; set; } = string.Empty;
        [Display(Name = "Tên kiểu dáng")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
    }

    public class ShapeImportDto
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string IsActive { get; set; } = null!;
    }
}
