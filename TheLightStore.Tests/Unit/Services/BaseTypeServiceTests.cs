using FluentAssertions;
using Moq;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Tests.Unit.Services;

/// <summary>
/// Unit tests for BaseTypeService
/// Tests cover all CRUD operations and error handling scenarios
/// </summary>
public class BaseTypeServiceTests : IDisposable
{
    private readonly BaseTypeFixture _fixture;

    public BaseTypeServiceTests()
    {
        _fixture = new BaseTypeFixture();
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
        var baseTypeService = _fixture.CreateBaseTypeService();
        var filterParams = new BaseTypeFilterParams { PageNumber = 1, PageSize = 10 };
        var paginatedData = _fixture.CreateTestPaginationModel();

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(paginatedData);

        // Act
        var result = await baseTypeService.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get base type list successfully");
        Assert.NotNull(result.Data);
        _fixture.MockBaseTypeRepository.Verify(x => x.GetAllAsync(filterParams), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithFilterByCode_ReturnsFilteredResults()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var filterParams = new BaseTypeFilterParams { Code = "BT_E27", PageNumber = 1, PageSize = 10 };
        var filteredData = _fixture.CreateTestPaginationModel(
            new List<BaseTypeGetDto> { _fixture.CreateTestBaseTypeGetDto(code: "BT_E27") }
        );

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ReturnsAsync(filteredData);

        // Act
        var result = await baseTypeService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var data = result.Data as PaginationModel<BaseTypeGetDto>;
        data?.Records.Should().AllSatisfy(bt => bt.Code.Should().Contain("BT_E27"));
    }

    [Fact]
    public async Task GetAll_WhenRepositoryThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var filterParams = new BaseTypeFilterParams();
        var errorMessage = "Database connection failed";

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetAllAsync(filterParams))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await baseTypeService.GetAll(filterParams);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Error getting base type list");
        result.Message.Should().Contain(errorMessage);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsSuccessResponseWithBaseTypeData()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var baseTypeId = 1;
        var baseTypeData = _fixture.CreateTestBaseTypeGetDto(id: baseTypeId);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByIdAsync(baseTypeId))
            .ReturnsAsync(baseTypeData);

        // Act
        var result = await baseTypeService.GetById(baseTypeId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Get base type successfully");
        var data = result.Data as BaseTypeGetDto;
        data?.Id.Should().Be(baseTypeId);
        _fixture.MockBaseTypeRepository.Verify(x => x.GetByIdAsync(baseTypeId), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsErrorResponse()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var invalidId = -1;

        // Act
        var result = await baseTypeService.GetById(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type ID must be greater than 0");
        _fixture.MockBaseTypeRepository.Verify(x => x.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var nonExistentId = 999;

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByIdAsync(nonExistentId))
            .ReturnsAsync((BaseTypeGetDto?)null);

        // Act
        var result = await baseTypeService.GetById(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Base type with ID {nonExistentId} not found");
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsSuccessResponseWithNewBaseTypeId()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var createDto = _fixture.CreateTestBaseTypeCreateDto();
        var newBaseTypeId = 1;

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync((BaseType?)null);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.CreateAsync(It.IsAny<BaseType>()))
            .ReturnsAsync(newBaseTypeId);

        // Act
        var result = await baseTypeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Base type created successfully");
        Assert.NotNull(result.Data);
        _fixture.MockBaseTypeRepository.Verify(x => x.CreateAsync(It.IsAny<BaseType>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithoutBaseTypeName_ReturnsValidationError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var createDto = new BaseTypeCreateDto { Name = "" }; // Empty name

        // Act
        var result = await baseTypeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type name is required");
        _fixture.MockBaseTypeRepository.Verify(x => x.CreateAsync(It.IsAny<BaseType>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsConflictError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var createDto = _fixture.CreateTestBaseTypeCreateDto(code: "BT_E27");
        var existingBaseType = _fixture.CreateTestBaseType(code: "BT_E27");

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByCodeAsync(createDto.Code!))
            .ReturnsAsync(existingBaseType);

        // Act
        var result = await baseTypeService.Create(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type with code 'BT_E27' already exists");
        _fixture.MockBaseTypeRepository.Verify(x => x.CreateAsync(It.IsAny<BaseType>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidDto_ReturnsSuccessResponse()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var updateDto = _fixture.CreateTestBaseTypeUpdateDto(id: 1);
        var existingBaseTypeDto = _fixture.CreateTestBaseTypeGetDto(id: 1);
        var existingBaseType = _fixture.CreateTestBaseType(id: 1);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync(existingBaseTypeDto);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByCodeAsync(existingBaseTypeDto.Code))
            .ReturnsAsync(existingBaseType);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.UpdateAsync(It.IsAny<BaseType>()))
            .ReturnsAsync(true);

        // Act
        var result = await baseTypeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Base type updated successfully");
        _fixture.MockBaseTypeRepository.Verify(x => x.UpdateAsync(It.IsAny<BaseType>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var updateDto = _fixture.CreateTestBaseTypeUpdateDto(id: -1);

        // Act
        var result = await baseTypeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type ID must be greater than 0");
        _fixture.MockBaseTypeRepository.Verify(x => x.UpdateAsync(It.IsAny<BaseType>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var updateDto = _fixture.CreateTestBaseTypeUpdateDto(id: 999);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.GetByIdAsync(updateDto.Id))
            .ReturnsAsync((BaseTypeGetDto?)null);

        // Act
        var result = await baseTypeService.Update(updateDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type with ID 999 not found");
    }

    #endregion

    #region Remove Tests

    [Fact]
    public async Task Remove_WithValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var baseTypeId = 1;

        _fixture.MockBaseTypeRepository
            .Setup(x => x.ExistsAsync(baseTypeId))
            .ReturnsAsync(true);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.DeleteAsync(baseTypeId))
            .ReturnsAsync(true);

        // Act
        var result = await baseTypeService.Remove(baseTypeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Base type deleted successfully");
        _fixture.MockBaseTypeRepository.Verify(x => x.DeleteAsync(baseTypeId), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ReturnsValidationError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var invalidId = 0;

        // Act
        var result = await baseTypeService.Remove(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Base type ID must be greater than 0");
        _fixture.MockBaseTypeRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var nonExistentId = 999;

        _fixture.MockBaseTypeRepository
            .Setup(x => x.ExistsAsync(nonExistentId))
            .ReturnsAsync(false);

        // Act
        var result = await baseTypeService.Remove(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Base type with ID {nonExistentId} not found");
        _fixture.MockBaseTypeRepository.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenDeleteFails_ReturnsErrorResponse()
    {
        // Arrange
        var baseTypeService = _fixture.CreateBaseTypeService();
        var baseTypeId = 1;

        _fixture.MockBaseTypeRepository
            .Setup(x => x.ExistsAsync(baseTypeId))
            .ReturnsAsync(true);

        _fixture.MockBaseTypeRepository
            .Setup(x => x.DeleteAsync(baseTypeId))
            .ReturnsAsync(false);

        // Act
        var result = await baseTypeService.Remove(baseTypeId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to delete base type");
    }

    #endregion
}
