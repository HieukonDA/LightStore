using Moq;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.Tests.Fixtures;

/// <summary>
/// Fixture for ShapeService testing - provides mocked dependencies and test data
/// </summary>
public class ShapeFixture
{
    public Mock<IShapeRepository> MockShapeRepository { get; set; } = new();

    public ShapeFixture()
    {
        Reset();
    }

    public void Reset()
    {
        MockShapeRepository.Reset();
    }

    public ShapeService CreateShapeService()
    {
        return new ShapeService(MockShapeRepository.Object);
    }

    public Shape CreateTestShape(long id = 1, string name = "Tròn", string code = "SH_ROUND")
    {
        return new Shape
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test shape",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public ShapeGetDto CreateTestShapeGetDto(long id = 1, string name = "Tròn", string code = "SH_ROUND")
    {
        return new ShapeGetDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Test shape",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test_user"
        };
    }

    public ShapeCreateDto CreateTestShapeCreateDto(string name = "Tròn", string code = "SH_ROUND")
    {
        return new ShapeCreateDto
        {
            Name = name,
            Code = code,
            Description = "Test shape",
            IsActive = true
        };
    }

    public ShapeUpdateDto CreateTestShapeUpdateDto(long id = 1, string name = "Tròn", string code = "SH_ROUND")
    {
        return new ShapeUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Updated test shape",
            IsActive = true
        };
    }

    public PaginationModel<ShapeGetDto> CreateTestPaginationModel(List<ShapeGetDto>? records = null)
    {
        records ??= new List<ShapeGetDto> { CreateTestShapeGetDto() };
        
        return new PaginationModel<ShapeGetDto>
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
