using FluentAssertions;
using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using TheLightStore.Tests.Fixtures;
using Xunit;

namespace TheLightStore.Tests.Unit.Services;

public class PromotionServiceTests
{
    private readonly PromotionFixture _fixture;

    public PromotionServiceTests()
    {
        _fixture = new PromotionFixture();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParams_ShouldReturnPaginatedData()
    {
        // Arrange
        var filterParams = new PromotionDto.PromotionFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockPromotionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<PromotionDto.PromotionFilterParams>()))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockPromotionRepository.Verify(r => r.GetAllAsync(It.IsAny<PromotionDto.PromotionFilterParams>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithCodeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filterParams = new PromotionDto.PromotionFilterParams
        {
            Code = "PROMO001",
            PageNumber = 1,
            PageSize = 10
        };
        var expectedPaginationModel = _fixture.CreateTestPaginationModel();
        _fixture.MockPromotionRepository
            .Setup(r => r.GetAllAsync(filterParams))
            .ReturnsAsync(expectedPaginationModel);

        var service = _fixture.CreatePromotionService();

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
        var filterParams = new PromotionDto.PromotionFilterParams
        {
            PageNumber = 1,
            PageSize = 10
        };
        _fixture.MockPromotionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<PromotionDto.PromotionFilterParams>()))
            .ThrowsAsync(new Exception("Database error"));

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.GetAll(filterParams);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnPromotion()
    {
        // Arrange
        long id = 1;
        var expectedPromotion = _fixture.CreateTestPromotionGetDto(id);
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(expectedPromotion);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockPromotionRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.GetById(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task GetById_WhenPromotionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((PromotionDto.PromotionGetDto?)null);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ShouldCreatePromotionAndReturnId()
    {
        // Arrange
        var createDto = _fixture.CreateTestPromotionCreateDto();
        long expectedId = 1;
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Promotion?)null);
        _fixture.MockPromotionRepository
            .Setup(r => r.CreateAsync(It.IsAny<Promotion>()))
            .ReturnsAsync(expectedId);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Create(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockPromotionRepository.Verify(r => r.CreateAsync(It.IsAny<Promotion>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithMissingName_ShouldReturnValidationError()
    {
        // Arrange
        var createDto = new PromotionDto.PromotionCreateDto
        {
            Name = string.Empty,
            Code = "PROMO002"
        };

        var service = _fixture.CreatePromotionService();

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
        var createDto = _fixture.CreateTestPromotionCreateDto(code: "PROMO001");
        var existingPromotion = _fixture.CreateTestPromotion(code: "PROMO001");
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByCodeAsync("PROMO001"))
            .ReturnsAsync(existingPromotion);

        var service = _fixture.CreatePromotionService();

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
        var updateDto = _fixture.CreateTestPromotionUpdateDto(1);
        var existingPromotion = _fixture.CreateTestPromotionGetDto(1);
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPromotion);
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((Promotion?)null);
        _fixture.MockPromotionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Promotion>()))
            .ReturnsAsync(true);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockPromotionRepository.Verify(r => r.UpdateAsync(It.IsAny<Promotion>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestPromotionUpdateDto(id: 0);
        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Update(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Update_WhenPromotionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var updateDto = _fixture.CreateTestPromotionUpdateDto(999);
        _fixture.MockPromotionRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((PromotionDto.PromotionGetDto?)null);

        var service = _fixture.CreatePromotionService();

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
        _fixture.MockPromotionRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockPromotionRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Remove(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SuccessResponseResult>();
        _fixture.MockPromotionRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Remove_WithInvalidId_ShouldReturnValidationError()
    {
        // Arrange
        long invalidId = 0;
        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Remove(invalidId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ErrorResponseResult>();
    }

    [Fact]
    public async Task Remove_WhenPromotionNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        long id = 999;
        _fixture.MockPromotionRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreatePromotionService();

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
        _fixture.MockPromotionRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockPromotionRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);

        var service = _fixture.CreatePromotionService();

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
        _fixture.MockPromotionRepository
            .Setup(r => r.ExistsAsync(id))
            .ReturnsAsync(true);
        _fixture.MockPromotionRepository
            .Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var service = _fixture.CreatePromotionService();

        // Act
        var result = await service.Remove(id);

        // Assert
        _fixture.MockPromotionRepository.Verify(r => r.ExistsAsync(id), Times.Once);
        _fixture.MockPromotionRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    #endregion
}
