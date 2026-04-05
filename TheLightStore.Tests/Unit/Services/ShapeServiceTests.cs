using FluentAssertions;
using Moq;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Tests.Unit.Services;

/// <summary>
/// Unit tests for ShapeService
/// Tests cover all CRUD operations and error handling scenarios
/// </summary>
public class ShapeServiceTests : IDisposable
{
    private readonly ShapeFixture _fixture;

    public ShapeServiceTests()
    {
        _fixture = new ShapeFixture();
    }

    public void Dispose()
    {
        _fixture.Reset();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParameters_ReturnsSuccessResponseWithPaginatedData()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var filterParams = new ShapeFilterParams { PageNumber = 1, PageSize = 10 };
        var paginatedData = _fixture.CreateTestPaginationModel();

        _fixture.MockShapeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(paginatedData);

        // Act
        var result = await shapeService.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get shape list successfully");
        Assert.NotNull(result.Data);
        _fixture.MockShapeRepository.Verify(x => x.GetAllAsync(filterParams), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithFilterByCode_ReturnsFilteredResults()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var filterParams = new ShapeFilterParams { Code = "SH_ROUND", PageNumber = 1, PageSize = 10 };
        var filteredData = _fixture.CreateTestPaginationModel(
            new List<ShapeGetDto> { _fixture.CreateTestShapeGetDto(code: "SH_ROUND") }
        );

        _fixture.MockShapeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(filteredData);

        // Act
        var result = await shapeService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.Data as PaginationModel<ShapeGetDto>;
        data?.Records.Should().AllSatisfy(s => s.Code.Should().Contain("SH_ROUND"));
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var filterParams = new ShapeFilterParams();
        var errorMessage = "Database connection failed";

        _fixture.MockShapeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await shapeService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Error getting shape list");
        result.Message.Should().Contain(errorMessage);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsSuccessResponseWithShapeData()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var shapeId = 1;
        var shapeData = _fixture.CreateTestShapeGetDto(id: shapeId);

        _fixture.MockShapeRepository
            .Setup(x => x.GetByIdAsync(shapeId))
            .ReturnsAsync(shapeData);

        // Act
        var result = await shapeService.GetById(shapeId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get shape successfully");
        var data = result.Data as ShapeGetDto;
        data?.Id.Should().Be(shapeId);
        _fixture.MockShapeRepository.Verify(x => x.GetByIdAsync(shapeId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsErrorResponse()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var invalidId = -1;

        // Act
        var result = await shapeService.GetById(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape ID must be greater than 0");
        _fixture.MockShapeRepository.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var nonExistentId = 999;

        _fixture.MockShapeRepository
            .Setup(x => x.GetByIdAsync(nonExistentId))
            .ReturnsAsync((ShapeGetDto?)null);

        // Act
        var result = await shapeService.GetById(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Shape with ID {nonExistentId} not found");
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsSuccessResponseWithNewShapeId()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var createDto = _fixture.CreateTestShapeCreateDto();
        var newShapeId = 1;

        _fixture.MockShapeRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync((Shape?)null);

        _fixture.MockShapeRepository
            .Setup(x => x.CreateAsync(It.IsAny<Shape>()))
            .ReturnsAsync(newShapeId);

        // Act
        var result = await shapeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Shape created successfully");
        Assert.NotNull(result.Data);
        _fixture.MockShapeRepository.Verify(x => x.CreateAsync(It.IsAny<Shape>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutShapeName_ReturnsValidationError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var createDto = new ShapeCreateDto { Name = "" }; // Empty name

        // Act
        var result = await shapeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape name is required");
        _fixture.MockShapeRepository.Verify(x => x.CreateAsync(It.IsAny<Shape>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsConflictError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var createDto = _fixture.CreateTestShapeCreateDto(code: "SH_ROUND");
        var existingShape = _fixture.CreateTestShape(code: "SH_ROUND");

        _fixture.MockShapeRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync(existingShape);

        // Act
        var result = await shapeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape with code 'SH_ROUND' already exists");
        _fixture.MockShapeRepository.Verify(x => x.CreateAsync(It.IsAny<Shape>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var updateDto = _fixture.CreateTestShapeUpdateDto(id: 1);
        var existingShapeDto = _fixture.CreateTestShapeGetDto(id: 1);
        var existingShape = _fixture.CreateTestShape(id: 1);

        _fixture.MockShapeRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync(existingShapeDto);

        _fixture.MockShapeRepository
            .Setup(x => x.GetByCodeAsync(existingShapeDto.Code))
            .ReturnsAsync(existingShape);

        _fixture.MockShapeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Shape>()))
            .ReturnsAsync(true);

        // Act
        var result = await shapeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Shape updated successfully");
        _fixture.MockShapeRepository.Verify(x => x.UpdateAsync(It.IsAny<Shape>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var updateDto = _fixture.CreateTestShapeUpdateDto(id: -1);

        // Act
        var result = await shapeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape ID must be greater than 0");
        _fixture.MockShapeRepository.Verify(x => x.UpdateAsync(It.IsAny<Shape>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var updateDto = _fixture.CreateTestShapeUpdateDto(id: 999);

        _fixture.MockShapeRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync((ShapeGetDto?)null);

        // Act
        var result = await shapeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape with ID 999 not found");
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var shapeId = 1;

        _fixture.MockShapeRepository
            .Setup(x => x.ExistsAsync(shapeId))
            .ReturnsAsync(true);

        _fixture.MockShapeRepository
            .Setup(x => x.DeleteAsync(shapeId))
            .ReturnsAsync(true);

        // Act
        var result = await shapeService.Remove(shapeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Shape deleted successfully");
        _fixture.MockShapeRepository.Verify(x => x.DeleteAsync(shapeId), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var invalidId = 0;

        // Act
        var result = await shapeService.Remove(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Shape ID must be greater than 0");
        _fixture.MockShapeRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var nonExistentId = 999;

        _fixture.MockShapeRepository
            .Setup(x => x.ExistsAsync(nonExistentId))
            .ReturnsAsync(false);

        // Act
        var result = await shapeService.Remove(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Shape with ID {nonExistentId} not found");
        _fixture.MockShapeRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenDeleteFails_ReturnsErrorResponse()
    {
        // Arrange
        var shapeService = _fixture.CreateShapeService();
        var shapeId = 1;

        _fixture.MockShapeRepository
            .Setup(x => x.ExistsAsync(shapeId))
            .ReturnsAsync(true);

        _fixture.MockShapeRepository
            .Setup(x => x.DeleteAsync(shapeId))
            .ReturnsAsync(false);

        // Act
        var result = await shapeService.Remove(shapeId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to delete shape");
    }

    #endregion
}
