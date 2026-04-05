using Moq;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.Tests.Fixtures;

/// <summary>
/// Fixture for BaseTypeService testing - provides mocked dependencies and test data
/// </summary>
public class BaseTypeFixture
{
    public Mock<IBaseTypeRepository> MockBaseTypeRepository { get; set; } = new();

    public BaseTypeFixture()
    {
        Reset();
    }

    public void Reset()
    {
        MockBaseTypeRepository.Reset();
    }

    public BaseTypeService CreateBaseTypeService()
    {
        return new BaseTypeService(MockBaseTypeRepository.Object);
    }

    public BaseType CreateTestBaseType(long id = 1, string name = "E27", string code = "BT_E27")
    {
        return new BaseType
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test base type",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public BaseTypeGetDto CreateTestBaseTypeGetDto(long id = 1, string name = "E27", string code = "BT_E27")
    {
        return new BaseTypeGetDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test base type",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public BaseTypeCreateDto CreateTestBaseTypeCreateDto(string name = "E27", string code = "BT_E27")
    {
        return new BaseTypeCreateDto
        {
            Name = name,
            Code = code,
            Description = "Test base type",
            IsActive = true
        };
    }

    public BaseTypeUpdateDto CreateTestBaseTypeUpdateDto(long id = 1, string name = "E27", string code = "BT_E27")
    {
        return new BaseTypeUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Updated test base type",
            IsActive = true
        };
    }

    public PaginationModel<BaseTypeGetDto> CreateTestPaginationModel(List<BaseTypeGetDto>? records = null)
    {
        records ??= new List<BaseTypeGetDto> { CreateTestBaseTypeGetDto() };
        
        return new PaginationModel<BaseTypeGetDto>
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
