using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Tests.Fixtures;

public class BrandFixture
{
    public Mock<IBrandRepository> MockBrandRepository { get; set; }

    public BrandFixture()
    {
        MockBrandRepository = new Mock<IBrandRepository>();
    }

    public BrandService CreateBrandService()
    {
        return new BrandService(MockBrandRepository.Object);
    }

    public void Reset()
    {
        MockBrandRepository.Reset();
    }

    public Brand CreateTestBrand(long id = 1, string code = "BRD001", string name = "Test Brand")
    {
        return new Brand
        {
            Id = id,
            Code = code,
            Name = name,
            Description = "Test Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public BrandDto.BrandGetDto CreateTestBrandGetDto(long id = 1, string code = "BRD001", string name = "Test Brand")
    {
        return new BrandDto.BrandGetDto
        {
            Id = id,
            Code = code,
            Name = name,
            Description = "Test Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public BrandDto.BrandCreateDto CreateTestBrandCreateDto(string name = "New Brand", string description = "New Description")
    {
        return new BrandDto.BrandCreateDto
        {
            Name = name,
            Description = description,
            IsActive = true
        };
    }

    public BrandDto.BrandUpdateDto CreateTestBrandUpdateDto(long id = 1, string name = "Updated Brand", string description = "Updated Description")
    {
        return new BrandDto.BrandUpdateDto
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = true
        };
    }

    public PaginationModel<BrandDto.BrandGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<BrandDto.BrandGetDto>
        {
            Records = new List<BrandDto.BrandGetDto>
            {
                CreateTestBrandGetDto(1, "BRD001", "Brand 1"),
                CreateTestBrandGetDto(2, "BRD002", "Brand 2")
            },
            Pagination = new Pagination
            {
                CurrentPage = 1,
                PerPage = 10,
                TotalRecords = 2,
                TotalPages = 1
            }
        };
    }
}
