using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Tests.Fixtures;

public class ProductDetailFixture
{
    public Mock<IProductDetailRepository> MockProductDetailRepository { get; set; }
    public Mock<IProductRepository> MockProductRepository { get; set; }

    public ProductDetailFixture()
    {
        MockProductDetailRepository = new Mock<IProductDetailRepository>();
        MockProductRepository = new Mock<IProductRepository>();
    }

    public ProductDetailService CreateProductDetailService()
    {
        return new ProductDetailService(MockProductDetailRepository.Object, MockProductRepository.Object);
    }

    public void Reset()
    {
        MockProductDetailRepository.Reset();
        MockProductRepository.Reset();
    }

    public ProductDetail CreateTestProductDetail(long id = 1, long productId = 1)
    {
        return new ProductDetail
        {
            Id = id,
            ProductId = productId,
            PowerId = 1,
            ColorTemperatureId = 1,
            ShapeId = 1,
            BaseTypeId = 1,
            SellingPrice = 100000,
            EarningPoints = 50,
            SoldQuantity = 10,
            Description = "Test description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }

    public ProductDetailDto.ProductDetailGetDto CreateTestProductDetailGetDto(long id = 1, long productId = 1)
    {
        return new ProductDetailDto.ProductDetailGetDto
        {
            Id = id,
            ProductId = productId,
            PowerId = 1,
            ColorTemperatureId = 1,
            ShapeId = 1,
            BaseTypeId = 1,
            SellingPrice = 100000,
            EarningPoints = 50,
            SoldQuantity = 10,
            Description = "Test description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            ProductName = "Test Product",
            PowerName = "50W",
            ColorTemperatureName = "Warm"
        };
    }

    public ProductDetailDto.ProductDetailCreateDto CreateTestProductDetailCreateDto(long productId = 1)
    {
        return new ProductDetailDto.ProductDetailCreateDto
        {
            ProductId = productId,
            PowerId = 1,
            ColorTemperatureId = 1,
            ShapeId = 1,
            BaseTypeId = 1,
            SellingPrice = 100000,
            EarningPoints = 50,
            SoldQuantity = 0,
            Description = "New product detail",
            IsActive = false
        };
    }

    public ProductDetailDto.ListProductDetailCreateDto CreateTestListProductDetailCreateDto()
    {
        return new ProductDetailDto.ListProductDetailCreateDto
        {
            ProductIds = new List<long> { 1, 2, 3 }
        };
    }

    public ProductDetailDto.ProductDetailUpdateDto CreateTestProductDetailUpdateDto(long id = 1)
    {
        return new ProductDetailDto.ProductDetailUpdateDto
        {
            Id = id,
            SellingPrice = 150000,
            EarningPoints = 75,
            SoldQuantity = 5,
            Description = "Updated description",
            IsActive = true
        };
    }

    public ProductDetailDto.ListProductDetailRemoveDto CreateTestListProductDetailRemoveDto()
    {
        return new ProductDetailDto.ListProductDetailRemoveDto
        {
            ProductDetailIds = new List<long> { 1, 2, 3 }
        };
    }

    public PaginationModel<ProductDetailDto.ProductDetailGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<ProductDetailDto.ProductDetailGetDto>
        {
            Records = new List<ProductDetailDto.ProductDetailGetDto>
            {
                CreateTestProductDetailGetDto(1, 1),
                CreateTestProductDetailGetDto(2, 2)
            },
            Pagination = new Pagination
            {
                CurrentPage = 1,
                PerPage = 10,
                TotalRecords = 2,
                TotalPages = 1
            }
        };
    }
}
