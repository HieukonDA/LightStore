using FluentAssertions;
using Moq;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Tests.Unit.Services;

/// <summary>
/// Unit tests for ColorTemperatureService
/// Tests cover all CRUD operations and error handling scenarios
/// </summary>
public class ColorTemperatureServiceTests : IDisposable
{
    private readonly ColorTemperatureFixture _fixture;

    public ColorTemperatureServiceTests()
    {
        _fixture = new ColorTemperatureFixture();
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
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var filterParams = new ColorTemperatureFilterParams { PageNumber = 1, PageSize = 10 };
        var paginatedData = _fixture.CreateTestPaginationModel();

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(paginatedData);

        // Act
        var result = await colorTemperatureService.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get color temperature list successfully");
        Assert.NotNull(result.Data);
        _fixture.MockColorTemperatureRepository.Verify(x => x.GetAllAsync(filterParams), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithFilterByCode_ReturnsFilteredResults()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var filterParams = new ColorTemperatureFilterParams { Code = "CT_6000K", PageNumber = 1, PageSize = 10 };
        var filteredData = _fixture.CreateTestPaginationModel(
            new List<ColorTemperatureGetDto> { _fixture.CreateTestColorTemperatureGetDto(code: "CT_6000K") }
        );

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(filteredData);

        // Act
        var result = await colorTemperatureService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.Data as PaginationModel<ColorTemperatureGetDto>;
        data?.Records.Should().AllSatisfy(ct => ct.Code.Should().Contain("CT_6000K"));
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var filterParams = new ColorTemperatureFilterParams();
        var errorMessage = "Database connection failed";

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await colorTemperatureService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Error getting color temperature list");
        result.Message.Should().Contain(errorMessage);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsSuccessResponseWithColorTemperatureData()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var colorTemperatureId = 1;
        var colorTemperatureData = _fixture.CreateTestColorTemperatureGetDto(id: colorTemperatureId);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByIdAsync(colorTemperatureId))
            .ReturnsAsync(colorTemperatureData);

        // Act
        var result = await colorTemperatureService.GetById(colorTemperatureId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get color temperature successfully");
        var data = result.Data as ColorTemperatureGetDto;
        data?.Id.Should().Be(colorTemperatureId);
        _fixture.MockColorTemperatureRepository.Verify(x => x.GetByIdAsync(colorTemperatureId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsErrorResponse()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var invalidId = -1;

        // Act
        var result = await colorTemperatureService.GetById(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature ID must be greater than 0");
        _fixture.MockColorTemperatureRepository.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var nonExistentId = 999;

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByIdAsync(nonExistentId))
            .ReturnsAsync((ColorTemperatureGetDto?)null);

        // Act
        var result = await colorTemperatureService.GetById(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Color temperature with ID {nonExistentId} not found");
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsSuccessResponseWithNewColorTemperatureId()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var createDto = _fixture.CreateTestColorTemperatureCreateDto();
        var newColorTemperatureId = 1;

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync((ColorTemperature?)null);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.CreateAsync(It.IsAny<ColorTemperature>()))
            .ReturnsAsync(newColorTemperatureId);

        // Act
        var result = await colorTemperatureService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Color temperature created successfully");
        Assert.NotNull(result.Data);
        _fixture.MockColorTemperatureRepository.Verify(x => x.CreateAsync(It.IsAny<ColorTemperature>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutColorTemperatureName_ReturnsValidationError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var createDto = new ColorTemperatureCreateDto { Name = "" }; // Empty name

        // Act
        var result = await colorTemperatureService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature name is required");
        _fixture.MockColorTemperatureRepository.Verify(x => x.CreateAsync(It.IsAny<ColorTemperature>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsConflictError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var createDto = _fixture.CreateTestColorTemperatureCreateDto(code: "CT_6000K");
        var existingColorTemperature = _fixture.CreateTestColorTemperature(code: "CT_6000K");

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync(existingColorTemperature);

        // Act
        var result = await colorTemperatureService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature with code 'CT_6000K' already exists");
        _fixture.MockColorTemperatureRepository.Verify(x => x.CreateAsync(It.IsAny<ColorTemperature>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var updateDto = _fixture.CreateTestColorTemperatureUpdateDto(id: 1);
        var existingColorTemperatureDto = _fixture.CreateTestColorTemperatureGetDto(id: 1);
        var existingColorTemperature = _fixture.CreateTestColorTemperature(id: 1);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync(existingColorTemperatureDto);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByCodeAsync(existingColorTemperatureDto.Code))
            .ReturnsAsync(existingColorTemperature);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ColorTemperature>()))
            .ReturnsAsync(true);

        // Act
        var result = await colorTemperatureService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Color temperature updated successfully");
        _fixture.MockColorTemperatureRepository.Verify(x => x.UpdateAsync(It.IsAny<ColorTemperature>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var updateDto = _fixture.CreateTestColorTemperatureUpdateDto(id: -1);

        // Act
        var result = await colorTemperatureService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature ID must be greater than 0");
        _fixture.MockColorTemperatureRepository.Verify(x => x.UpdateAsync(It.IsAny<ColorTemperature>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var updateDto = _fixture.CreateTestColorTemperatureUpdateDto(id: 999);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync((ColorTemperatureGetDto?)null);

        // Act
        var result = await colorTemperatureService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature with ID 999 not found");
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var colorTemperatureId = 1;

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.ExistsAsync(colorTemperatureId))
            .ReturnsAsync(true);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.DeleteAsync(colorTemperatureId))
            .ReturnsAsync(true);

        // Act
        var result = await colorTemperatureService.Remove(colorTemperatureId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Color temperature deleted successfully");
        _fixture.MockColorTemperatureRepository.Verify(x => x.DeleteAsync(colorTemperatureId), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var invalidId = 0;

        // Act
        var result = await colorTemperatureService.Remove(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Color temperature ID must be greater than 0");
        _fixture.MockColorTemperatureRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var nonExistentId = 999;

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.ExistsAsync(nonExistentId))
            .ReturnsAsync(false);

        // Act
        var result = await colorTemperatureService.Remove(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Color temperature with ID {nonExistentId} not found");
        _fixture.MockColorTemperatureRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenDeleteFails_ReturnsErrorResponse()
    {
        // Arrange
        var colorTemperatureService = _fixture.CreateColorTemperatureService();
        var colorTemperatureId = 1;

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.ExistsAsync(colorTemperatureId))
            .ReturnsAsync(true);

        _fixture.MockColorTemperatureRepository
            .Setup(x => x.DeleteAsync(colorTemperatureId))
            .ReturnsAsync(false);

        // Act
        var result = await colorTemperatureService.Remove(colorTemperatureId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to delete color temperature");
    }

    #endregion
}
