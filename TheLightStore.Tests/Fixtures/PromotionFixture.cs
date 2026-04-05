using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Tests.Fixtures;

public class PromotionFixture
{
    public Mock<IPromotionRepository> MockPromotionRepository { get; set; }

    public PromotionFixture()
    {
        MockPromotionRepository = new Mock<IPromotionRepository>();
    }

    public PromotionService CreatePromotionService()
    {
        return new PromotionService(MockPromotionRepository.Object);
    }

    public void Reset()
    {
        MockPromotionRepository.Reset();
    }

    public Promotion CreateTestPromotion(long id = 1, string code = "PROMO001", string name = "Test Promotion")
    {
        return new Promotion
        {
            Id = id,
            Code = code,
            Name = name,
            PercentDiscount = 10,
            StartedDate = DateTime.UtcNow.AddDays(-1),
            EndedDate = DateTime.UtcNow.AddDays(10),
            Status = "Active",
            Description = "Test Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public PromotionDto.PromotionGetDto CreateTestPromotionGetDto(long id = 1, string code = "PROMO001", string name = "Test Promotion")
    {
        return new PromotionDto.PromotionGetDto
        {
            Id = id,
            Code = code,
            Name = name,
            PercentDiscount = 10,
            StartedDate = DateTime.UtcNow.AddDays(-1),
            EndedDate = DateTime.UtcNow.AddDays(10),
            Status = "Active",
            Description = "Test Description",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public PromotionDto.PromotionCreateDto CreateTestPromotionCreateDto(string name = "New Promotion", string code = "PROMO002")
    {
        return new PromotionDto.PromotionCreateDto
        {
            Name = name,
            Code = code,
            PercentDiscount = 15,
            StartedDate = DateTime.UtcNow,
            EndedDate = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Description = "Test Description",
            IsActive = true
        };
    }

    public PromotionDto.PromotionUpdateDto CreateTestPromotionUpdateDto(long id = 1, string name = "Updated Promotion", string code = "PROMO001")
    {
        return new PromotionDto.PromotionUpdateDto
        {
            Id = id,
            Name = name,
            Code = code,
            PercentDiscount = 20,
            StartedDate = DateTime.UtcNow,
            EndedDate = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Description = "Updated Description",
            IsActive = true
        };
    }

    public PaginationModel<PromotionDto.PromotionGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<PromotionDto.PromotionGetDto>
        {
            Records = new List<PromotionDto.PromotionGetDto>
            {
                CreateTestPromotionGetDto(1, "PROMO001", "Promotion 1"),
                CreateTestPromotionGetDto(2, "PROMO002", "Promotion 2")
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
