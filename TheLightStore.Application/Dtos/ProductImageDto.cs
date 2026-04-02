using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class ProductImageDto
{
    public class ProductImageCreateDto
    {
        [Required]
        public long ProductId { get; set; }
        [Required]
        public Guid FileId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductImageUpdateDto : ProductImageCreateDto
    {
        public long Id { get; set; }
    }

    public class ProductImageGetDto : ProductImageUpdateDto
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }

    public class ProductImageFilterParams
    {
        public long? ProductId { get; set; }
        public Guid? FileId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class ProductImageExportDto
    {
        public long Id { get; set; }
        [Display(Name = "ID sản phẩm")]
        public long ProductId { get; set; }
        [Display(Name = "ID tệp")]
        public Guid FileId { get; set; }
        [Display(Name = "Tên tệp")]
        public string FileName { get; set; } = string.Empty;
        [Display(Name = "Đường dẫn")]
        public string FilePath { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
    }

    public class ProductImageImportDto
    {
        public string? Id { get; set; }
        [Required]
        public long ProductId { get; set; }
        [Required]
        public string FileId { get; set; } = null!;
        public string IsActive { get; set; } = null!;
    }
}
