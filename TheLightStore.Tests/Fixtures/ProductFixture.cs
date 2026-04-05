using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Infrastructure.Persistence;

namespace TheLightStore.Tests.Fixtures;

public class ProductFixture
{
    public Mock<IProductRepository> MockProductRepository { get; set; }
    public Mock<ICategoryRepository> MockCategoryRepository { get; set; }
    public Mock<IBrandRepository> MockBrandRepository { get; set; }
    public Mock<IPromotionRepository> MockPromotionRepository { get; set; }

    public ProductFixture()
    {
        MockProductRepository = new Mock<IProductRepository>();
        MockCategoryRepository = new Mock<ICategoryRepository>();
        MockBrandRepository = new Mock<IBrandRepository>();
        MockPromotionRepository = new Mock<IPromotionRepository>();
    }

    public ProductService CreateProductService()
    {
        return new ProductService(
            MockProductRepository.Object,
            MockCategoryRepository.Object,
            MockBrandRepository.Object,
            MockPromotionRepository.Object);
    }

    public void Reset()
    {
        MockProductRepository.Reset();
        MockCategoryRepository.Reset();
        MockBrandRepository.Reset();
        MockPromotionRepository.Reset();
    }

    public Product CreateTestProduct(long id = 1, string code = "PRD001", long categoryId = 1)
    {
        return new Product
        {
            Id = id,
            Code = code,
            ProductType = "SELF_PRODUCED",
            Name = "Test Product",
            CategoryId = categoryId,
            BrandId = 1,
            IsInBusiness = true,
            IsOrderedOnline = true,
            IsPackaged = true,
            Description = "Test product description",
            Position = "A1",
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }

    public ProductDto.ProductGetDto CreateTestProductGetDto(long id = 1, string code = "PRD001")
    {
        return new ProductDto.ProductGetDto
        {
            Id = id,
            Code = code,
            ProductType = "SELF_PRODUCED",
            Name = "Test Product",
            CategoryId = 1,
            BrandId = 1,
            IsInBusiness = true,
            IsOrderedOnline = true,
            IsPackaged = true,
            Description = "Test product description",
            Position = "A1",
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }

    public ProductDto.ProductCreateDto CreateTestProductCreateDto(long categoryId = 1)
    {
        return new ProductDto.ProductCreateDto
        {
            Code = "PRD" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            ProductType = "SELF_PRODUCED",
            Name = "New Product",
            CategoryId = categoryId,
            BrandId = 1,
            IsInBusiness = true,
            IsOrderedOnline = true,
            IsPackaged = true,
            Description = "New product description",
            Position = "A2",
            ImageUrl = "https://example.com/image2.jpg",
            IsActive = true
        };
    }

    public ProductDto.UpdateExtraDto CreateTestUpdateExtraDto(long id = 1)
    {
        return new ProductDto.UpdateExtraDto
        {
            Id = id,
            Code = "PRD001",
            ProductType = "SELF_PRODUCED",
            Name = "Updated Product",
            CategoryId = 1,
            BrandId = 1,
            IsInBusiness = true,
            IsOrderedOnline = true,
            IsPackaged = true,
            Description = "Updated description",
            Position = "A3",
            ImageUrl = "https://example.com/updated.jpg",
            IsActive = true
        };
    }

    public ProductDto.ProductUpdateDto CreateTestProductUpdateDto(long id = 1)
    {
        return new ProductDto.ProductUpdateDto
        {
            Id = id,
            Code = "PRD001",
            Name = "Updated by User",
            Description = "User updated description",
            CategoryId = 1,
            BrandId = 1,
            IsInBusiness = true,
            IsOrderedOnline = true,
            IsPackaged = true,
            ImageUrl = "https://example.com/user-updated.jpg",
            IsActive = true
        };
    }

    public PaginationModel<ProductDto.ProductGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<ProductDto.ProductGetDto>
        {
            Records = new List<ProductDto.ProductGetDto>
            {
                CreateTestProductGetDto(1, "PRD001"),
                CreateTestProductGetDto(2, "PRD002")
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

    public CategoryDto.CategoryGetDto CreateTestCategoryGetDto(long id = 1, string code = "CAT001", string name = "Test Category")
    {
        return new CategoryDto.CategoryGetDto
        {
            Id = id,
            Code = code,
            Name = name,
            Description = "Test Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }

    public BrandDto.BrandGetDto CreateTestBrandGetDto(long id = 1, string code = "BRAND001", string name = "Test Brand")
    {
        return new BrandDto.BrandGetDto
        {
            Id = id,
            Code = code,
            Name = name,
            Description = "Test Brand Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }

    public PromotionDto.PromotionGetDto CreateTestPromotionGetDto(long id = 1, string code = "PROMO001", string name = "Test Promotion")
    {
        return new PromotionDto.PromotionGetDto
        {
            Id = id,
            Code = code,
            Name = name,
            Description = "Test Promotion",
            PercentDiscount = 10,
            StartedDate = DateTime.UtcNow,
            EndedDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };
    }
}
