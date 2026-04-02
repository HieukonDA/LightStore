using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class CategoryDto
{
    public class CategoryCreateDto
    {
        [Required]
        public required string Name { get; set; }
        public string? Code { get; set; }
        public long? ParentId { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CategoryUpdateDto : CategoryCreateDto
    {
        public long Id { get; set; }
    }

    public class CategoryGetDto : CategoryUpdateDto
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CategoryFilterParams
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long? ParentId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class CategoryExportDto
    {
        public long Id { get; set; }
        [Display(Name = "Mã phân loại")]
        public string Code { get; set; } = string.Empty;
        [Display(Name = "Tên phân loại")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Phân loại cha")]
        public long? ParentId { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
    }

    public class CategoryImportDto
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public long? ParentId { get; set; }
        public string? Description { get; set; }
        public string IsActive { get; set; } = null!;
    }
}
