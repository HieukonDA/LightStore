using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class ProductDto
{
    public class ProductBaseDto
    {
        public string ProductType { get; set; } = "SELF_PRODUCED";
        [Required]
        public required string Name { get; set; }
        public string Code { get; set; } = string.Empty;
        public bool IsInBusiness { get; set; }
        public bool IsOrderedOnline { get; set; }
        public bool IsPackaged { get; set; }
        public string? Description { get; set; }
        public string? Position { get; set; }
        public long CategoryId { get; set; }
        public long? BrandId { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductCreateDto : ProductBaseDto
    {
        public List<long>? ProductImageIds { get; set; }
        public List<ProductDetailDto.ProductDetailCreateDto> ProductDetails { get; set; } = [];
        public long? PromotionId { get; set; }
    }

    public class ProductUpdateDto : ProductBaseDto
    {
        public long Id { get; set; }
        public List<long>? ProductImageIds { get; set; }
        public List<ProductDetailDto.ProductDetailUpdateDto> ProductDetails { get; set; } = [];
    }

    public class UpdateExtraDto : ProductBaseDto
    {
        public long Id { get; set; }
        public List<long>? ProductImageIds { get; set; }
        public List<ProductDetailDto.ProductDetailUpdateDto> ProductDetails { get; set; } = [];
        public long? PromotionId { get; set; }
    }

    public class ProductGetDto : ProductBaseDto
    {
        public long Id { get; set; }
        public double? AverageRatingPoint { get; set; }
        public long? TotalSoldQuantity { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public CategoryDto.CategoryGetDto? Category { get; set; }
        public BrandDto.BrandGetDto? Brand { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public List<ProductDetailDto.ProductDetailGetDto>? ProductDetails { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public PromotionDto.PromotionGetDto? Promotion { get; set; }
        public List<ProductImageDto.ProductImageGetDto> ProductImages { get; set; } = [];
    }

    public class ProductFilterParams
    {
        public string? SearchString { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public string? ProductType { get; set; }
        public bool? IsInBusiness { get; set; }
        public bool? IsOrderedOnline { get; set; }
        public bool? IsPackaged { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class ProductShortDto : ProductBaseDto
    {
        public long Id { get; set; }
        public double? AverageRatingPoint { get; set; }
        public long? TotalSoldQuantity { get; set; }
        public CategoryDto.CategoryGetDto? Category { get; set; }
        public BrandDto.BrandGetDto? Brand { get; set; }
        public List<ProductDetailDto.ProductDetailShortDto>? ProductDetails { get; set; }
    }

    public class ListProductRemoveDto
    {
        [Required]
        public required List<long> ProductIds { get; set; }
    }

    public class ProductExportDto
    {
        public long Id { get; set; }
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Mã sản phẩm")]
        public string Code { get; set; } = string.Empty;
        [Display(Name = "Loại sản phẩm")]
        public string ProductType { get; set; } = string.Empty;
        [Display(Name = "Phân loại")]
        public string CategoryName { get; set; } = string.Empty;
        [Display(Name = "Mã phân loại")]
        public string CategoryCode { get; set; } = string.Empty;
        [Display(Name = "Thương hiệu")]
        public string BrandName { get; set; } = string.Empty;
        [Display(Name = "Mã thương hiệu")]
        public string BrandCode { get; set; } = string.Empty;
        [Display(Name = "Bán trong cửa hàng")]
        public bool IsInBusiness { get; set; }
        [Display(Name = "Bán online")]
        public bool IsOrderedOnline { get; set; }
        [Display(Name = "Có đóng gói")]
        public bool IsPackaged { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Vị trí")]
        public string Position { get; set; } = string.Empty;
        [Display(Name = "Đánh giá")]
        public double AverageRatingPoint { get; set; }
        [Display(Name = "Đã bán")]
        public long TotalSoldQuantity { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Tạo bởi")]
        public string CreatedBy { get; set; } = string.Empty;
        [Display(Name = "Cập nhật lần cuối")]
        public DateTime? UpdatedDate { get; set; }
        [Display(Name = "Cập nhật bởi")]
        public string UpdatedBy { get; set; } = string.Empty;
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
        [Display(Name = "Chi tiết sản phẩm (JSON)")]
        public string ProductDetails { get; set; } = string.Empty;
    }

    public class ProductImportDto
    {
        public long? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string Code { get; set; } = string.Empty;
        public string ProductType { get; set; } = "SELF_PRODUCED";
        [Required]
        public string CategoryCode { get; set; } = null!;
        public string? BrandCode { get; set; }
        public string IsInBusiness { get; set; } = null!;
        public string IsOrderedOnline { get; set; } = null!;
        public string IsPackaged { get; set; } = null!;
        public string IsActive { get; set; } = null!;
        public string? Description { get; set; }
        public string? Position { get; set; }
        public List<ProductDetailDto.ProductDetailImportDto> ProductDetails { get; set; } = [];
    }
}
