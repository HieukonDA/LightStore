using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Tests.Fixtures;

public class CategoryFixture
{
    public Mock<ICategoryRepository> MockCategoryRepository { get; set; }

    public CategoryFixture()
    {
        MockCategoryRepository = new Mock<ICategoryRepository>();
    }

    public CategoryService CreateCategoryService()
    {
        return new CategoryService(MockCategoryRepository.Object);
    }

    public void Reset()
    {
        MockCategoryRepository.Reset();
    }

    public Category CreateTestCategory(long id = 1, string code = "CAT001", string name = "Test Category")
    {
        return new Category
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

    public CategoryDto.CategoryGetDto CreateTestCategoryGetDto(long id = 1, string code = "CAT001", string name = "Test Category")
    {
        return new CategoryDto.CategoryGetDto
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

    public CategoryDto.CategoryCreateDto CreateTestCategoryCreateDto(string name = "New Category", string code = "CAT002")
    {
        return new CategoryDto.CategoryCreateDto
        {
            Name = name,
            Code = code,
            Description = "Test Description",
            IsActive = true
        };
    }

    public CategoryDto.CategoryUpdateDto CreateTestCategoryUpdateDto(long id = 1, string name = "Updated Category", string code = "CAT001")
    {
        return new CategoryDto.CategoryUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            Description = "Updated Description",
            IsActive = true
        };
    }

    public PaginationModel<CategoryDto.CategoryGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<CategoryDto.CategoryGetDto>
        {
            Records = new List<CategoryDto.CategoryGetDto>
            {
                CreateTestCategoryGetDto(1, "CAT001", "Category 1"),
                CreateTestCategoryGetDto(2, "CAT002", "Category 2")
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
