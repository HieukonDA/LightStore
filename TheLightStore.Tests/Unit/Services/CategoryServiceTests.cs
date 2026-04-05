using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class CategoryServiceTests
{
    private readonly CategoryFixture _fixture;

    public CategoryServiceTests()
    {
        _fixture = new CategoryFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new CategoryDto.CategoryFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockCategoryRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CategoryDto.CategoryFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockCategoryRepository.Verify(r => r.GetAllAsync(It.IsAny<CategoryDto.CategoryFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithCodeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new CategoryDto.CategoryFilterParams
        {
            Code = "CAT001",
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockCategoryRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreateCategoryService();

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
        var filterParams = new CategoryDto.CategoryFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        _fixture.MockCategoryRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CategoryDto.CategoryFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        long id = 1;
        var expectedCategory = _fixture.CreateTestCategoryGetDto(id);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedCategory);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockCategoryRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.GetById(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task GetById_WhenCategoryNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((CategoryDto.CategoryGetDto?)null);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateCategoryAndReturnId()
    {
        // Arrange
        var createDto = _fixture.CreateTestCategoryCreateDto();
        long expectedId = 1;
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Category?)null);
        _fixture.MockCategoryRepository
            .Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync(expectedId);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockCategoryRepository.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithMissingName_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new CategoryDto.CategoryCreateDto
        {
            Name = string.Empty,
            Code = "CAT002"
        };

        var service = _fixture.CreateCategoryService();

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
        var createDto = _fixture.CreateTestCategoryCreateDto(code: "CAT001");
        var existingCategory = _fixture.CreateTestCategory(code: "CAT001");
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByCodeAsync("CAT001"))
            .ReturnsAsync(existingCategory);

        var service = _fixture.CreateCategoryService();

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
        var updateDto = _fixture.CreateTestCategoryUpdateDto(1);
        var existingCategory = _fixture.CreateTestCategoryGetDto(1);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingCategory);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Category?)null);
        _fixture.MockCategoryRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .ReturnsAsync(true);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockCategoryRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestCategoryUpdateDto(id: 0);
        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Update_WhenCategoryNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestCategoryUpdateDto(999);
        _fixture.MockCategoryRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((CategoryDto.CategoryGetDto?)null);

        var service = _fixture.CreateCategoryService();

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
        _fixture.MockCategoryRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockCategoryRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockCategoryRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Remove(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_WhenCategoryNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockCategoryRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreateCategoryService();

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
        _fixture.MockCategoryRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockCategoryRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreateCategoryService();

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
        _fixture.MockCategoryRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockCategoryRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreateCategoryService();

        // Act
        var result = await service.Remove(id);

        // Assert
        _fixture.MockCategoryRepository.Verify(r => r.ExistsAsync(id), Times.Once);
        _fixture.MockCategoryRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    #endregion
}
