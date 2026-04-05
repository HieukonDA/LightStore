using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class ProductPromotionService : IProductPromotionService
{
    private readonly IProductPromotionRepository _productPromotionRepository;

    public ProductPromotionService(IProductPromotionRepository productPromotionRepository)
    {
        _productPromotionRepository = productPromotionRepository;
    }

    public async Task<ResponseResult> GetAll(ProductPromotionDto.ProductPromotionFilterParams filterParams)
    {
        try
        {
            var result = await _productPromotionRepository.GetAllAsync(filterParams);
            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> GetProductsByPromotionId(long promotionId)
    {
        if (promotionId <= 0)
            return new ErrorResponseResult("ID khuyến mãi phải lớn hơn 0");

        try
        {
            var products = await _productPromotionRepository.GetProductsByPromotionIdAsync(promotionId);
            return new SuccessResponseResult(new ProductPromotionDto.ProductsByPromotionResponse
            {
                Products = products
            });
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> Create(ProductPromotionDto.ProductPromotionCreateDto createDto)
    {
        if (createDto.ProductId <= 0)
            return new ErrorResponseResult("ID sản phẩm phải lớn hơn 0");

        if (createDto.PromotionId <= 0)
            return new ErrorResponseResult("ID khuyến mãi phải lớn hơn 0");

        var productPromotion = ProductPromotionMappings.CreateDtoToEntity(createDto);
        productPromotion.CreatedDate = DateTime.UtcNow;
        productPromotion.IsActive = createDto.IsActive ?? true;

        var id = await _productPromotionRepository.CreateAsync(productPromotion);
        return new SuccessResponseResult(id);
    }

    public async Task<ResponseResult> CreateMultiple(ProductPromotionDto.ListProductPromotionCreateDto createDto)
    {
        if (createDto.ProductIds == null || createDto.ProductIds.Count == 0)
            return new ErrorResponseResult("Danh sách sản phẩm không được rỗng");

        if (createDto.PromotionId <= 0)
            return new ErrorResponseResult("ID khuyến mãi phải lớn hơn 0");

        try
        {
            var productPromotions = new List<ProductPromotion>();
            foreach (var productId in createDto.ProductIds)
            {
                productPromotions.Add(new ProductPromotion
                {
                    ProductId = productId,
                    PromotionId = createDto.PromotionId,
                    IsActive = createDto.IsActive ?? true,
                    CreatedDate = DateTime.UtcNow
                });
            }

            foreach (var pp in productPromotions)
            {
                await _productPromotionRepository.CreateAsync(pp);
            }

            return new SuccessResponseResult("Thêm sản phẩm vào khuyến mãi thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> UpdateMultiple(ProductPromotionDto.ListProductPromotionUpdateDto updateDto)
    {
        if (updateDto.ProductPromotionIds == null || updateDto.ProductPromotionIds.Count == 0)
            return new ErrorResponseResult("Danh sách ID sản phẩm khuyến mãi không được rỗng");

        if (updateDto.PromotionId <= 0)
            return new ErrorResponseResult("ID khuyến mãi phải lớn hơn 0");

        try
        {
            foreach (var id in updateDto.ProductPromotionIds)
            {
                var productPromotion = await _productPromotionRepository.GetByIdAsync(id);
                if (productPromotion != null)
                {
                    var updatedEntity = new ProductPromotion
                    {
                        Id = id,
                        ProductId = productPromotion.ProductId,
                        PromotionId = updateDto.PromotionId,
                        IsActive = updateDto.IsActive ?? true,
                        UpdatedDate = DateTime.UtcNow
                    };

                    await _productPromotionRepository.UpdateAsync(updatedEntity);
                }
            }

            return new SuccessResponseResult("Cập nhật sản phẩm khuyến mãi thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> RemoveMultiple(ProductPromotionDto.ListProductPromotionRemoveDto removeDto)
    {
        if (removeDto.ProductPromotionIds == null || removeDto.ProductPromotionIds.Count == 0)
            return new ErrorResponseResult("Danh sách ID sản phẩm khuyến mãi không được rỗng");

        try
        {
            foreach (var id in removeDto.ProductPromotionIds)
            {
                await _productPromotionRepository.DeleteAsync(id);
            }

            return new SuccessResponseResult("Xóa sản phẩm khuyến mãi thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }
}
