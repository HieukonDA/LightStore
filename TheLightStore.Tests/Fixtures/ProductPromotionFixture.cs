using Moq;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Tests.Fixtures;

public class ProductPromotionFixture
{
    public Mock<IProductPromotionRepository> MockProductPromotionRepository { get; set; }

    public ProductPromotionFixture()
    {
        MockProductPromotionRepository = new Mock<IProductPromotionRepository>();
    }

    public ProductPromotionService CreateProductPromotionService()
    {
        return new ProductPromotionService(MockProductPromotionRepository.Object);
    }

    public void Reset()
    {
        MockProductPromotionRepository.Reset();
    }

    public ProductPromotion CreateTestProductPromotion(long id = 1, long productId = 1, long promotionId = 1)
    {
        return new ProductPromotion
        {
            Id = id,
            ProductId = productId,
            PromotionId = promotionId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public ProductPromotionDto.ProductPromotionGetDto CreateTestProductPromotionGetDto(long id = 1, long productId = 1, long promotionId = 1)
    {
        return new ProductPromotionDto.ProductPromotionGetDto
        {
            Id = id,
            ProductId = productId,
            PromotionId = promotionId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "TestUser"
        };
    }

    public ProductPromotionDto.ProductPromotionCreateDto CreateTestProductPromotionCreateDto(long productId = 2, long promotionId = 2)
    {
        return new ProductPromotionDto.ProductPromotionCreateDto
        {
            ProductId = productId,
            PromotionId = promotionId,
            IsActive = true
        };
    }

    public ProductPromotionDto.ListProductPromotionCreateDto CreateTestListProductPromotionCreateDto(long promotionId = 1)
    {
        return new ProductPromotionDto.ListProductPromotionCreateDto
        {
            ProductIds = new List<long> { 1, 2, 3 },
            PromotionId = promotionId,
            IsActive = true
        };
    }

    public ProductPromotionDto.ListProductPromotionUpdateDto CreateTestListProductPromotionUpdateDto(long promotionId = 1)
    {
        return new ProductPromotionDto.ListProductPromotionUpdateDto
        {
            ProductPromotionIds = new List<long> { 1, 2 },
            PromotionId = promotionId,
            IsActive = true
        };
    }

    public ProductPromotionDto.ListProductPromotionRemoveDto CreateTestListProductPromotionRemoveDto()
    {
        return new ProductPromotionDto.ListProductPromotionRemoveDto
        {
            ProductPromotionIds = new List<long> { 1, 2, 3 }
        };
    }

    public PaginationModel<ProductPromotionDto.ProductPromotionGetDto> CreateTestPaginationModel()
    {
        return new PaginationModel<ProductPromotionDto.ProductPromotionGetDto>
        {
            Records = new List<ProductPromotionDto.ProductPromotionGetDto>
            {
                CreateTestProductPromotionGetDto(1, 1, 1),
                CreateTestProductPromotionGetDto(2, 2, 1)
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
