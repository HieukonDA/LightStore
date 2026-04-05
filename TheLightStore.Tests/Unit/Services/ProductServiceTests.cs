using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class ProductServiceTests
{
    private readonly ProductFixture _fixture;

    public ProductServiceTests()
    {
        _fixture = new ProductFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new ProductDto.ProductFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductDto.ProductFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductRepository.Verify(r => r.GetAllAsync(It.IsAny<ProductDto.ProductFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithCategoryFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new ProductDto.ProductFilterParams
        {
            CategoryId = 1,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ShouldReturnErrorResponse()
    {
        // Arrange
        var filterParams = new ProductDto.ProductFilterParams();
        _fixture.MockProductRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductDto.ProductFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        long id = 1;
        var expectedDto = _fixture.CreateTestProductGetDto(1);
        _fixture.MockProductRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedDto);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetById(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        long id = 999;
        _fixture.MockProductRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((ProductDto.ProductGetDto?)null);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region CreateByAdmin Tests

    [Fact]
    public async Task CreateByAdmin_WithValidDto_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = _fixture.CreateTestProductCreateDto();
        var categoryDto = _fixture.CreateTestCategoryGetDto(id: createDto.CategoryId);
        var brandDto = _fixture.CreateTestBrandGetDto(id: createDto.BrandId.Value);
        
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(createDto.CategoryId))
            .ReturnsAsync(categoryDto);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(createDto.BrandId.Value))
            .ReturnsAsync(brandDto);
        _fixture.MockProductRepository
            .Setup(r => r.ExistsByCodeAsync(createDto.Code, 0))
            .ReturnsAsync(false);
        _fixture.MockProductRepository
            .Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync(1);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.CreateByAdmin(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task CreateByAdmin_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = _fixture.CreateTestProductCreateDto();
        createDto.Name = string.Empty;

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.CreateByAdmin(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task CreateByAdmin_WithDuplicateCode_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = _fixture.CreateTestProductCreateDto();
        _fixture.MockProductRepository
            .Setup(r => r.ExistsByCodeAsync(createDto.Code, 0))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.CreateByAdmin(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task CreateByAdmin_WithInvalidCategory_ShouldReturnErrorResponse()
    {
        // Arrange
        var createDto = _fixture.CreateTestProductCreateDto();
        _fixture.MockProductRepository
            .Setup(r => r.ExistsByCodeAsync(createDto.Code, 0))
            .ReturnsAsync(false);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((CategoryDto.CategoryGetDto?)null);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.CreateByAdmin(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region UpdateByAdmin Tests

    [Fact]
    public async Task UpdateByAdmin_WithValidDto_ShouldUpdateSuccessfully()
    {
        // Arrange
        var updateDto = _fixture.CreateTestUpdateExtraDto(1);
        var existingProduct = _fixture.CreateTestProduct(1);
        var categoryDto = _fixture.CreateTestCategoryGetDto(id: updateDto.CategoryId);
        var brandDto = _fixture.CreateTestBrandGetDto(id: updateDto.BrandId.Value);
        
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(updateDto.Id))
            .ReturnsAsync(existingProduct);
        _fixture.MockProductRepository
            .Setup(r => r.ExistsByCodeAsync(updateDto.Code, updateDto.Id))
            .ReturnsAsync(false);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(updateDto.CategoryId))
            .ReturnsAsync(categoryDto);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(updateDto.BrandId.Value))
            .ReturnsAsync(brandDto);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByAdmin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task UpdateByAdmin_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestUpdateExtraDto(0);
        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByAdmin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task UpdateByAdmin_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        var updateDto = _fixture.CreateTestUpdateExtraDto(999);
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(999))
            .ReturnsAsync((Product?)null);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByAdmin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region UpdateByUserLogin Tests

    [Fact]
    public async Task UpdateByUserLogin_WithValidDto_ShouldUpdateSuccessfully()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductUpdateDto(1);
        var existingProduct = _fixture.CreateTestProduct(1);
        
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(updateDto.Id))
            .ReturnsAsync(existingProduct);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByUserLogin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task UpdateByUserLogin_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductUpdateDto(0);
        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByUserLogin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task UpdateByUserLogin_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductUpdateDto(999);
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(999))
            .ReturnsAsync((Product?)null);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.UpdateByUserLogin(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region RemoveByAdmin Tests

    [Fact]
    public async Task RemoveByAdmin_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        long id = 1;
        var existingProduct = _fixture.CreateTestProduct(1);
        existingProduct.ProductDetails = []; // Empty details to allow deletion
        
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(id))
            .ReturnsAsync(existingProduct);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.RemoveByAdmin(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task RemoveByAdmin_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductService();

        // Act
        var result = await service.RemoveByAdmin(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task RemoveByAdmin_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        long id = 999;
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(id))
            .ReturnsAsync((Product?)null);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.RemoveByAdmin(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task RemoveByAdmin_WithProductDetails_ShouldReturnErrorResponse()
    {
        // Arrange
        long id = 1;
        var existingProduct = _fixture.CreateTestProduct(1);
        existingProduct.ProductDetails = new List<ProductDetail>
        {
            new ProductDetail { Id = 1, ProductId = id }
        };
        
        _fixture.MockProductRepository
            .Setup(r => r.GetProductWithDetailsAsync(id))
            .ReturnsAsync(existingProduct);

        var service = _fixture.CreateProductService();

        // Act
        var result = await service.RemoveByAdmin(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion
}
