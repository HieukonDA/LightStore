using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class PromotionDto
{
    public class PromotionCreateDto
    {
        [Required]
        public required string Name { get; set; }
        public long PercentDiscount { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime EndedDate { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }

    public class PromotionUpdateDto : PromotionCreateDto
    {
        public long Id { get; set; }
    }

    public class PromotionGetDto : PromotionUpdateDto
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class PromotionFilterParams
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? EndedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class PromotionExportDto
    {
        public long Id { get; set; }
        [Display(Name = "Mã khuyến mãi")]
        public string Code { get; set; } = string.Empty;
        [Display(Name = "Tên khuyến mãi")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Phần trăm giảm")]
        public long PercentDiscount { get; set; }
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartedDate { get; set; }
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndedDate { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
    }

    public class PromotionImportDto
    {
        public string? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public long PercentDiscount { get; set; }
        public string StartedDate { get; set; } = null!;
        public string EndedDate { get; set; } = null!;
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string Code { get; set; } = string.Empty;
        public string IsActive { get; set; } = null!;
    }
}
