using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class BrandServiceTests
{
    private readonly BrandFixture _fixture;

    public BrandServiceTests()
    {
        _fixture = new BrandFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new BrandDto.BrandFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockBrandRepository
            .Setup(r => r.GetAllAsync(It.IsAny<BrandDto.BrandFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockBrandRepository.Verify(r => r.GetAllAsync(It.IsAny<BrandDto.BrandFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new BrandDto.BrandFilterParams
        {
            Name = "Brand 1",
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockBrandRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateBrandService();

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
        var filterParams = new BrandDto.BrandFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        _fixture.MockBrandRepository
            .Setup(r => r.GetAllAsync(It.IsAny<BrandDto.BrandFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnBrand()
    {
        // Arrange
        long id = 1;
        var expectedBrand = _fixture.CreateTestBrandGetDto(id);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedBrand);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockBrandRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.GetById(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task GetById_WhenBrandNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((BrandDto.BrandGetDto?)null);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateBrandAndReturnId()
    {
        // Arrange
        var createDto = _fixture.CreateTestBrandCreateDto();
        long expectedId = 1;
        _fixture.MockBrandRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Brand?)null);
        _fixture.MockBrandRepository
            .Setup(r => r.CreateAsync(It.IsAny<Brand>()))
            .ReturnsAsync(expectedId);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockBrandRepository.Verify(r => r.CreateAsync(It.IsAny<Brand>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithMissingName_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new BrandDto.BrandCreateDto
        {
            Name = string.Empty,
            Description = "Test"
        };

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ShouldReturnConflictError()
    {
        // Arrange
        var createDto = _fixture.CreateTestBrandCreateDto(description: "BRD001");
        var existingBrand = _fixture.CreateTestBrand(code: "BRD001");
        _fixture.MockBrandRepository
            .Setup(r => r.GetByCodeAsync("BRD001"))
            .ReturnsAsync(existingBrand);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Create(createDto);

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
        var updateDto = _fixture.CreateTestBrandUpdateDto(1);
        var existingBrand = _fixture.CreateTestBrandGetDto(1);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingBrand);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Brand?)null);
        _fixture.MockBrandRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Brand>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockBrandRepository.Verify(r => r.UpdateAsync(It.IsAny<Brand>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestBrandUpdateDto(id: 0);
        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Update_WhenBrandNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestBrandUpdateDto(999);
        _fixture.MockBrandRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BrandDto.BrandGetDto?)null);

        var service = _fixture.CreateBrandService();

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
        _fixture.MockBrandRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockBrandRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockBrandRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Remove(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_WhenBrandNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockBrandRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_WhenDeleteOperationFails_ShouldReturnErrorResponse()
    {
        // Arrange
        long id = 1;
        _fixture.MockBrandRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockBrandRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_CheckExistenceBeforeDeletion_ShouldVerifyExistenceCheck()
    {
        // Arrange
        long id = 1;
        _fixture.MockBrandRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockBrandRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreateBrandService();

        // Act
        var result = await service.Remove(id);

        // Assert
        _fixture.MockBrandRepository.Verify(r => r.ExistsAsync(id), Times.Once);
        _fixture.MockBrandRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    #endregion
}
