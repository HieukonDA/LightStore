using Moq;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.Tests.Fixtures;

/// <summary>
/// Fixture for ColorTemperatureService testing - provides mocked dependencies and test data
/// </summary>
public class ColorTemperatureFixture
{
    public Mock<IColorTemperatureRepository> MockColorTemperatureRepository { get; set; } = new();

    public ColorTemperatureFixture()
    {
        Reset();
    }

    public void Reset()
    {
        MockColorTemperatureRepository.Reset();
    }

    public ColorTemperatureService CreateColorTemperatureService()
    {
        return new ColorTemperatureService(MockColorTemperatureRepository.Object);
    }

    public ColorTemperature CreateTestColorTemperature(long id = 1, string name = "Trắng 6000K", string code = "CT_6000K")
    {
        return new ColorTemperature
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test color temperature",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public ColorTemperatureGetDto CreateTestColorTemperatureGetDto(long id = 1, string name = "Trắng 6000K", string code = "CT_6000K")
    {
        return new ColorTemperatureGetDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test color temperature",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public ColorTemperatureCreateDto CreateTestColorTemperatureCreateDto(string name = "Trắng 6000K", string code = "CT_6000K")
    {
        return new ColorTemperatureCreateDto
        {
            Name = name,
            Code = code,
            Description = "Test color temperature",
            IsActive = true
        };
    }

    public ColorTemperatureUpdateDto CreateTestColorTemperatureUpdateDto(long id = 1, string name = "Trắng 6000K", string code = "CT_6000K")
    {
        return new ColorTemperatureUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Updated test color temperature",
            IsActive = true
        };
    }

    public PaginationModel<ColorTemperatureGetDto> CreateTestPaginationModel(List<ColorTemperatureGetDto>? records = null)
    {
        records ??= new List<ColorTemperatureGetDto> { CreateTestColorTemperatureGetDto() };
        
        return new PaginationModel<ColorTemperatureGetDto>
        {
            Records = records,
            Pagination = new Pagination
            {
                TotalRecords = records.Count,
                TotalPages = 1,
                CurrentPage = 1,
                PerPage = 10,
                NextPage = null,
                PrevPage = null
            }
        };
    }
}
