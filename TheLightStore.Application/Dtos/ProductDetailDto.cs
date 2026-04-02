using System;
using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Application.Dtos;

public class ProductDetailDto
{
    public class ProductDetailCreateDto
    {
        [Required]
        public long ProductId { get; set; }
        public long? PowerId { get; set; }
        public long? ColorTemperatureId { get; set; }
        public long? ShapeId { get; set; }
        public long? BaseTypeId { get; set; }
        public long? CurrencyId { get; set; }
        public decimal SellingPrice { get; set; }
        public long EarningPoints { get; set; }
        public long? SoldQuantity { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ListProductDetailCreateDto
    {
        [Required]
        public required List<long> ProductIds { get; set; }
    }

    public class ListProductDetailRemoveDto
    {
        [Required]
        public required List<long> ProductDetailIds { get; set; }
    }

    public class ProductDetailUpdateDto
    {
        public long Id { get; set; }
        public decimal SellingPrice { get; set; }
        public long EarningPoints { get; set; }
        public long? SoldQuantity { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductDetailGetDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? PowerId { get; set; }
        public long? ColorTemperatureId { get; set; }
        public long? ShapeId { get; set; }
        public long? BaseTypeId { get; set; }
        public long? CurrencyId { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public long EarningPoints { get; set; }
        public long? SoldQuantity { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? ProductName { get; set; }
        public string? PowerName { get; set; }
        public string? ColorTemperatureName { get; set; }
        public string? ShapeName { get; set; }
        public string? BaseTypeName { get; set; }
    }

    public class ProductDetailFilterParams
    {
        public long? ProductId { get; set; }
        public long? PowerId { get; set; }
        public long? ColorTemperatureId { get; set; }
        public long? ShapeId { get; set; }
        public long? BaseTypeId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class RemainingProductQuantity
    {
        public long? SoldQuantity { get; set; }
        public long? RemainingQuantity { get; set; }
    }

    public class ProductDetailShortDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? PowerId { get; set; }
        public long? ColorTemperatureId { get; set; }
        public long? ShapeId { get; set; }
        public long? BaseTypeId { get; set; }
        public decimal SellingPrice { get; set; }
        public long EarningPoints { get; set; }
        public string? PowerName { get; set; }
        public string? ColorTemperatureName { get; set; }
        public string? ShapeName { get; set; }
        public string? BaseTypeName { get; set; }
    }

    public class ProductDetailExportDto
    {
        public long Id { get; set; }
        [Display(Name = "ID sản phẩm")]
        public long ProductId { get; set; }
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;
        [Display(Name = "Công suất")]
        public string PowerName { get; set; } = string.Empty;
        [Display(Name = "Nhiệt độ màu")]
        public string ColorTemperatureName { get; set; } = string.Empty;
        [Display(Name = "Kiểu dáng")]
        public string ShapeName { get; set; } = string.Empty;
        [Display(Name = "Loại đuôi")]
        public string BaseTypeName { get; set; } = string.Empty;
        [Display(Name = "Giá bán")]
        public decimal SellingPrice { get; set; }
        [Display(Name = "Điểm thưởng")]
        public long EarningPoints { get; set; }
        [Display(Name = "Đã bán")]
        public long? SoldQuantity { get; set; }
        [Display(Name = "Giá sau giảm")]
        public decimal? DiscountedPrice { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = string.Empty;
    }

    public class ProductDetailImportDto
    {
        public long? Id { get; set; }
        [Required]
        public long ProductId { get; set; }
        public long? PowerId { get; set; }
        public long? ColorTemperatureId { get; set; }
        public long? ShapeId { get; set; }
        public long? BaseTypeId { get; set; }
        public decimal SellingPrice { get; set; }
        public long EarningPoints { get; set; }
        public long? SoldQuantity { get; set; }
        public string? Description { get; set; }
        public string DetailIsActive { get; set; } = null!;
    }
}
