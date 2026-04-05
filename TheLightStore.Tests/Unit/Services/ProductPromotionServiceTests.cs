using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class ProductPromotionServiceTests
{
    private readonly ProductPromotionFixture _fixture;

    public ProductPromotionServiceTests()
    {
        _fixture = new ProductPromotionFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new ProductPromotionDto.ProductPromotionFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductPromotionDto.ProductPromotionFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.GetAllAsync(It.IsAny<ProductPromotionDto.ProductPromotionFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithPromotionIdFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new ProductPromotionDto.ProductPromotionFilterParams
        {
            PromotionId = 1,
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateProductPromotionService();

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
        var filterParams = new ProductPromotionDto.ProductPromotionFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<ProductPromotionDto.ProductPromotionFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetProductsByPromotionId Tests

    [Fact]
    public async Task GetProductsByPromotionId_WithValidPromotionId_ShouldReturnProducts()
    {
        // Arrange
        long promotionId = 1;
        var expectedProducts = new List<ProductPromotionDto.ProductsByPromotionDto>
        {
            new ProductPromotionDto.ProductsByPromotionDto { Id = 1, Name = "Product 1" },
            new ProductPromotionDto.ProductsByPromotionDto { Id = 2, Name = "Product 2" }
        };
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetProductsByPromotionIdAsync(promotionId))
            .ReturnsAsync(expectedProducts);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.GetProductsByPromotionId(promotionId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.GetProductsByPromotionIdAsync(promotionId), Times.Once);
    }

    [Fact]
    public async Task GetProductsByPromotionId_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.GetProductsByPromotionId(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task GetProductsByPromotionId_WhenRepositoryThrowsException_ShouldReturnErrorResponse()
    {
        // Arrange
        long promotionId = 1;
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetProductsByPromotionIdAsync(promotionId))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.GetProductsByPromotionId(promotionId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnId()
    {
        // Arrange
        var createDto = _fixture.CreateTestProductPromotionCreateDto();
        long expectedId = 1;
        _fixture.MockProductPromotionRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProductPromotion>()))
            .ReturnsAsync(expectedId);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.CreateAsync(It.IsAny<ProductPromotion>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidProductId_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new ProductPromotionDto.ProductPromotionCreateDto
        {
            ProductId = 0,
            PromotionId = 1
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Create_WithInvalidPromotionId_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new ProductPromotionDto.ProductPromotionCreateDto
        {
            ProductId = 1,
            PromotionId = 0
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.Create(createDto);

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
        var createDto = _fixture.CreateTestListProductPromotionCreateDto();
        _fixture.MockProductPromotionRepository
            .Setup(r => r.CreateAsync(It.IsAny<ProductPromotion>()))
            .ReturnsAsync(1);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.CreateAsync(It.IsAny<ProductPromotion>()), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateMultiple_WithEmptyProductIds_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new ProductPromotionDto.ListProductPromotionCreateDto
        {
            ProductIds = new List<long>(),
            PromotionId = 1
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task CreateMultiple_WithInvalidPromotionId_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new ProductPromotionDto.ListProductPromotionCreateDto
        {
            ProductIds = new List<long> { 1, 2 },
            PromotionId = 0
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.CreateMultiple(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region UpdateMultiple Tests

    [Fact]
    public async Task UpdateMultiple_WithValidDto_ShouldUpdateSuccessfully()
    {
        // Arrange
        var updateDto = _fixture.CreateTestListProductPromotionUpdateDto();
        var existingRecord = _fixture.CreateTestProductPromotionGetDto(1);
        _fixture.MockProductPromotionRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(existingRecord);
        _fixture.MockProductPromotionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ProductPromotion>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.UpdateMultiple(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.UpdateAsync(It.IsAny<ProductPromotion>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateMultiple_WithEmptyProductPromotionIds_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = new ProductPromotionDto.ListProductPromotionUpdateDto
        {
            ProductPromotionIds = new List<long>(),
            PromotionId = 1
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.UpdateMultiple(updateDto);

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
        var removeDto = _fixture.CreateTestListProductPromotionRemoveDto();
        _fixture.MockProductPromotionRepository
            .Setup(r => r.DeleteAsync(It.IsAny<long>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.RemoveMultiple(removeDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockProductPromotionRepository.Verify(r => r.DeleteAsync(It.IsAny<long>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RemoveMultiple_WithEmptyProductPromotionIds_ShouldReturnValidationError()
    {
        // Arrange
        var removeDto = new ProductPromotionDto.ListProductPromotionRemoveDto
        {
            ProductPromotionIds = new List<long>()
        };

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.RemoveMultiple(removeDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task RemoveMultiple_WhenDeleteFails_ShouldReturnErrorResponse()
    {
        // Arrange
        var removeDto = _fixture.CreateTestListProductPromotionRemoveDto();
        _fixture.MockProductPromotionRepository
            .Setup(r => r.DeleteAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateProductPromotionService();

        // Act
        var result = await service.RemoveMultiple(removeDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion
}
