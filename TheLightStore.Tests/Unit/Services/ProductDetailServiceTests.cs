using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class ProductDetailServiceTests
{
    private readonly ProductDetailFixture _fixture;

    public ProductDetailServiceTests()
    {
        _fixture = new ProductDetailFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new ProductDetailDto.ProductDetailFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductDetailDto.ProductDetailFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.GetAllAsync(It.IsAny<ProductDetailDto.ProductDetailFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithProductIdFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new ProductDetailDto.ProductDetailFilterParams
        {
            ProductId = 1,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductDetailService();

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
        var filterParams = new ProductDetailDto.ProductDetailFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductDetailDto.ProductDetailFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnProductDetail()
    {
        // Arrange
        long id = 1;
        var expectedDto = _fixture.CreateTestProductDetailGetDto(1);
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedDto);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductDetailService();

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
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((ProductDetailDto.ProductDetailGetDto?)null);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region CreateMultiple Tests

    [Fact]
    public async Task CreateMultiple_WithValidDto_ShouldCreateSuccessfully()
    {
        // Arrange
        var createDto = _fixture.CreateTestListProductDetailCreateDto();
        var productDto = new ProductDto.ProductGetDto { Id = 1, Code = "PRD001", Name = "Test Product" };
        _fixture.MockProductRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(productDto);
        _fixture.MockProductDetailRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProductDetail>()))
            .ReturnsAsync(1);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.CreateAsync(It.IsAny<ProductDetail>()), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateMultiple_WithEmptyProductIds_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new ProductDetailDto.ListProductDetailCreateDto
        {
            ProductIds = new List<long>()
        };

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task CreateMultiple_WithInvalidProductId_ShouldReturnErrorResponse()
    {
        // Arrange
        var createDto = _fixture.CreateTestListProductDetailCreateDto();
        _fixture.MockProductRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync((ProductDto.ProductGetDto?)null);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateSuccessfully()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductDetailUpdateDto(1);
        _fixture.MockProductDetailRepository
            .Setup(r => r.ExistsAsync(updateDto.Id))
            .ReturnsAsync(true);
        _fixture.MockProductDetailRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ProductDetail>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
    }

    [Fact]
    public async Task Update_WithInvalidSellingPrice_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductDetailUpdateDto(1);
        updateDto.SellingPrice = -100;

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Update_WithInvalidEarningPoints_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductDetailUpdateDto(1);
        updateDto.EarningPoints = -10;

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        var updateDto = _fixture.CreateTestProductDetailUpdateDto(999);
        _fixture.MockProductDetailRepository
            .Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        long id = 1;
        _fixture.MockProductDetailRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockProductDetailRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Remove(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        long id = 999;
        _fixture.MockProductDetailRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region RemoveMultiple Tests

    [Fact]
    public async Task RemoveMultiple_WithValidIds_ShouldDeleteSuccessfully()
    {
        // Arrange
        var removeDto = _fixture.CreateTestListProductDetailRemoveDto();
        _fixture.MockProductDetailRepository
            .Setup(r => r.DeleteAsync(It.IsAny<long>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.RemoveMultiple(removeDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.DeleteAsync(It.IsAny<long>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RemoveMultiple_WithEmptyProductDetailIds_ShouldReturnValidationError()
    {
        // Arrange
        var removeDto = new ProductDetailDto.ListProductDetailRemoveDto
        {
            ProductDetailIds = new List<long>()
        };

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.RemoveMultiple(removeDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region RemainingProductQuantity Tests

    [Fact]
    public async Task RemainingProductQuantity_WithValidProductId_ShouldReturnQuantities()
    {
        // Arrange
        long productId = 1;
        var productDetails = new List<ProductDetail>
        {
            _fixture.CreateTestProductDetail(1, productId),
            _fixture.CreateTestProductDetail(2, productId)
        };
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetProductDetailsByProductIdAsync(productId))
            .ReturnsAsync(productDetails);

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.RemainingProductQuantity(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductDetailRepository.Verify(r => r.GetProductDetailsByProductIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task RemainingProductQuantity_WithInvalidProductId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.RemainingProductQuantity(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task RemainingProductQuantity_WhenNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        long productId = 999;
        _fixture.MockProductDetailRepository
            .Setup(r => r.GetProductDetailsByProductIdAsync(productId))
            .ReturnsAsync(new List<ProductDetail>());

        var service = _fixture.CreateProductDetailService();

        // Act
        var result = await service.RemainingProductQuantity(productId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion
}
