using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class BrandDto
{
    public class BrandCreateDto
    {
        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class BrandUpdateDto : BrandCreateDto
    {
        public long Id { get; set; }
    }

    public class BrandGetDto : BrandUpdateDto
    {
        public string? Code { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class BrandFilterParams
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class BrandExportDto
    {
        public long Id { get; set; }

        [Display(Name = "Mã thương hiệu")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Tên thương hiệu")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
    }

    public class BrandImportDto
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string IsActive { get; set; } = null!;
    }
}
