using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class ProductPromotionDto
{
    public class ProductPromotionCreateDto
    {
        [Required]
        public long ProductId { get; set; }
        [Required]
        public long PromotionId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductPromotionUpdateDto : ProductPromotionCreateDto
    {
        public long Id { get; set; }
    }

    public class ProductPromotionGetDto : ProductPromotionUpdateDto
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public PromotionDto.PromotionGetDto? Promotion { get; set; }
        public ProductDto.ProductGetDto? Product { get; set; }
    }

    public class ProductPromotionFilterParams
    {
        public long? ProductId { get; set; }
        public long? PromotionId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class ListProductPromotionCreateDto
    {
        [Required]
        public List<long> ProductIds { get; set; } = [];
        [Required]
        public long PromotionId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ListProductPromotionUpdateDto
    {
        [Required]
        public List<long> ProductPromotionIds { get; set; } = [];
        [Required]
        public long PromotionId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ListProductPromotionRemoveDto
    {
        [Required]
        public List<long> ProductPromotionIds { get; set; } = [];
    }

    public class ProductsByPromotionResponse
    {
        public PromotionDto.PromotionGetDto? Promotion { get; set; }
        public List<ProductsByPromotionDto> Products { get; set; } = [];
    }

    public class ProductsByPromotionDto
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public long? BrandId { get; set; }
        public string? BrandName { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductForGroupSelectDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? CategoryName { get; set; }
        public bool IsHasPromotion { get; set; }
    }

    public class ProductPromotionExportDto
    {
        public long Id { get; set; }
        [Display(Name = "ID sản phẩm")]
        public long ProductId { get; set; }
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;
        [Display(Name = "ID khuyến mãi")]
        public long PromotionId { get; set; }
        [Display(Name = "Tên khuyến mãi")]
        public string PromotionName { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
    }

    public class ProductPromotionImportDto
    {
        public long? Id { get; set; }
        [Required]
        public long ProductId { get; set; }
        [Required]
        public long PromotionId { get; set; }
        public string IsActive { get; set; } = null!;
    }
}
