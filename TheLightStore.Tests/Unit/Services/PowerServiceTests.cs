using FluentAssertions;
using Moq;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Tests.Unit.Services;

/// <summary>
/// Unit tests for PowerService
/// Tests cover all CRUD operations and error handling scenarios
/// </summary>
public class PowerServiceTests : IDisposable
{
    private readonly PowerFixture _fixture;

    public PowerServiceTests()
    {
        _fixture = new PowerFixture();
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
        var powerService = _fixture.CreatePowerService();
        var filterParams = new PowerFilterParams { PageNumber = 1, PageSize = 10 };
        var paginatedData = _fixture.CreateTestPaginationModel();

        _fixture.MockPowerRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(paginatedData);

        // Act
        var result = await powerService.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get power list successfully");
        Assert.NotNull(result.Data);
        _fixture.MockPowerRepository.Verify(x => x.GetAllAsync(filterParams), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithFilterByCode_ReturnsFilteredResults()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var filterParams = new PowerFilterParams { Code = "PW_09", PageNumber = 1, PageSize = 10 };
        var filteredData = _fixture.CreateTestPaginationModel(
            new List<PowerGetDto> { _fixture.CreateTestPowerGetDto(code: "PW_09") }
        );

        _fixture.MockPowerRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(filteredData);

        // Act
        var result = await powerService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.Data as PaginationModel<PowerGetDto>;
        data?.Records.Should().AllSatisfy(p => p.Code.Should().Contain("PW_09"));
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var filterParams = new PowerFilterParams();
        var errorMessage = "Database connection failed";

        _fixture.MockPowerRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await powerService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Error getting power list");
        result.Message.Should().Contain(errorMessage);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsSuccessResponseWithPowerData()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var powerId = 1;
        var powerData = _fixture.CreateTestPowerGetDto(id: powerId);

        _fixture.MockPowerRepository
            .Setup(x => x.GetByIdAsync(powerId))
            .ReturnsAsync(powerData);

        // Act
        var result = await powerService.GetById(powerId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get power successfully");
        var data = result.Data as PowerGetDto;
        data?.Id.Should().Be(powerId);
        _fixture.MockPowerRepository.Verify(x => x.GetByIdAsync(powerId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsErrorResponse()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var invalidId = -1;

        // Act
        var result = await powerService.GetById(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power ID must be greater than 0");
        _fixture.MockPowerRepository.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var nonExistentId = 999;

        _fixture.MockPowerRepository
            .Setup(x => x.GetByIdAsync(nonExistentId))
            .ReturnsAsync((PowerGetDto?)null);

        // Act
        var result = await powerService.GetById(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Power with ID {nonExistentId} not found");
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsSuccessResponseWithNewPowerId()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var createDto = _fixture.CreateTestPowerCreateDto();
        var newPowerId = 1;

        _fixture.MockPowerRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync((Power?)null);

        _fixture.MockPowerRepository
            .Setup(x => x.CreateAsync(It.IsAny<Power>()))
            .ReturnsAsync(newPowerId);

        // Act
        var result = await powerService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Power created successfully");
        Assert.NotNull(result.Data);
        _fixture.MockPowerRepository.Verify(x => x.CreateAsync(It.IsAny<Power>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutPowerName_ReturnsValidationError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var createDto = new PowerCreateDto { Name = "" }; // Empty name

        // Act
        var result = await powerService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power name is required");
        _fixture.MockPowerRepository.Verify(x => x.CreateAsync(It.IsAny<Power>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsConflictError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var createDto = _fixture.CreateTestPowerCreateDto(code: "PW_09");
        var existingPower = _fixture.CreateTestPower(code: "PW_09");

        _fixture.MockPowerRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync(existingPower);

        // Act
        var result = await powerService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power with code 'PW_09' already exists");
        _fixture.MockPowerRepository.Verify(x => x.CreateAsync(It.IsAny<Power>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var updateDto = _fixture.CreateTestPowerUpdateDto(id: 1);
        var existingPowerDto = _fixture.CreateTestPowerGetDto(id: 1);
        var existingPower = _fixture.CreateTestPower(id: 1);

        _fixture.MockPowerRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync(existingPowerDto);

        _fixture.MockPowerRepository
            .Setup(x => x.GetByCodeAsync(existingPowerDto.Code))
            .ReturnsAsync(existingPower);

        _fixture.MockPowerRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Power>()))
            .ReturnsAsync(true);

        // Act
        var result = await powerService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Power updated successfully");
        _fixture.MockPowerRepository.Verify(x => x.UpdateAsync(It.IsAny<Power>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var updateDto = _fixture.CreateTestPowerUpdateDto(id: -1);

        // Act
        var result = await powerService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power ID must be greater than 0");
        _fixture.MockPowerRepository.Verify(x => x.UpdateAsync(It.IsAny<Power>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var updateDto = _fixture.CreateTestPowerUpdateDto(id: 999);

        _fixture.MockPowerRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync((PowerGetDto?)null);

        // Act
        var result = await powerService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power with ID 999 not found");
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var powerId = 1;

        _fixture.MockPowerRepository
            .Setup(x => x.ExistsAsync(powerId))
            .ReturnsAsync(true);

        _fixture.MockPowerRepository
            .Setup(x => x.DeleteAsync(powerId))
            .ReturnsAsync(true);

        // Act
        var result = await powerService.Remove(powerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Power deleted successfully");
        _fixture.MockPowerRepository.Verify(x => x.DeleteAsync(powerId), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var invalidId = 0;

        // Act
        var result = await powerService.Remove(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Power ID must be greater than 0");
        _fixture.MockPowerRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var nonExistentId = 999;

        _fixture.MockPowerRepository
            .Setup(x => x.ExistsAsync(nonExistentId))
            .ReturnsAsync(false);

        // Act
        var result = await powerService.Remove(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Power with ID {nonExistentId} not found");
        _fixture.MockPowerRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenDeleteFails_ReturnsErrorResponse()
    {
        // Arrange
        var powerService = _fixture.CreatePowerService();
        var powerId = 1;

        _fixture.MockPowerRepository
            .Setup(x => x.ExistsAsync(powerId))
            .ReturnsAsync(true);

        _fixture.MockPowerRepository
            .Setup(x => x.DeleteAsync(powerId))
            .ReturnsAsync(false);

        // Act
        var result = await powerService.Remove(powerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to delete power");
    }

    #endregion
}
