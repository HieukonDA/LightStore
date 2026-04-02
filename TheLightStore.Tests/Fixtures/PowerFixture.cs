using Moq;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.Tests.Fixtures;

/// <summary>
/// Fixture for PowerService testing - provides mocked dependencies and test data
/// </summary>
public class PowerFixture
{
    public Mock<IPowerRepository> MockPowerRepository { get; set; } = new();

    public PowerFixture()
    {
        Reset();
    }

    public void Reset()
    {
        MockPowerRepository.Reset();
    }

    public PowerService CreatePowerService()
    {
        return new PowerService(MockPowerRepository.Object);
    }

    public Power CreateTestPower(long id = 1, string name = "9W", string code = "PW_09")
    {
        return new Power
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test power",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public PowerGetDto CreateTestPowerGetDto(long id = 1, string name = "9W", string code = "PW_09")
    {
        return new PowerGetDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test power",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public PowerCreateDto CreateTestPowerCreateDto(string name = "9W", string code = "PW_09")
    {
        return new PowerCreateDto
        {
            Name = name,
            Code = code,
            Description = "Test power",
            IsActive = true
        };
    }

    public PowerUpdateDto CreateTestPowerUpdateDto(long id = 1, string name = "9W", string code = "PW_09")
    {
        return new PowerUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Updated test power",
            IsActive = true
        };
    }

    public PaginationModel<PowerGetDto> CreateTestPaginationModel(List<PowerGetDto>? records = null)
    {
        records ??= new List<PowerGetDto> { CreateTestPowerGetDto() };
        
        return new PaginationModel<PowerGetDto>
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
