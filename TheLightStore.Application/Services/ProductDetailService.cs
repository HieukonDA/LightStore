using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class ProductDetailService : IProductDetailService
{
    private readonly IProductDetailRepository _productDetailRepository;
    private readonly IProductRepository _productRepository;

    public ProductDetailService(IProductDetailRepository productDetailRepository, IProductRepository productRepository)
    {
        _productDetailRepository = productDetailRepository;
        _productRepository = productRepository;
    }

    public async Task<ResponseResult> GetAll(ProductDetailDto.ProductDetailFilterParams filterParams)
    {
        try
        {
            var result = await _productDetailRepository.GetAllAsync(filterParams);
            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> GetById(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID sản phẩm chi tiết phải lớn hơn 0");

        try
        {
            var result = await _productDetailRepository.GetByIdAsync(id);
            if (result == null)
                return new ErrorResponseResult("Không tìm thấy sản phẩm chi tiết");

            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> CreateMultiple(ProductDetailDto.ListProductDetailCreateDto createDto)
    {
        if (createDto.ProductIds == null || createDto.ProductIds.Count == 0)
            return new ErrorResponseResult("Danh sách sản phẩm không được rỗng");

        try
        {
            var productDetailsToCreate = new List<ProductDetail>();
            foreach (var productId in createDto.ProductIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                    return new ErrorResponseResult($"Sản phẩm với ID {productId} không tồn tại");

                var productDetail = new ProductDetail
                {
                    ProductId = productId,
                    SellingPrice = 0,
                    EarningPoints = 0,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                };

                productDetailsToCreate.Add(productDetail);
            }

            foreach (var pd in productDetailsToCreate)
            {
                await _productDetailRepository.CreateAsync(pd);
            }

            return new SuccessResponseResult($"Tạo {productDetailsToCreate.Count} chi tiết sản phẩm thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> Update(ProductDetailDto.ProductDetailUpdateDto updateDto)
    {
        if (updateDto.Id <= 0)
            return new ErrorResponseResult("ID sản phẩm chi tiết phải lớn hơn 0");

        if (updateDto.SellingPrice < 0)
            return new ErrorResponseResult("Giá bán phải lớn hơn hoặc bằng 0");

        if (updateDto.EarningPoints < 0)
            return new ErrorResponseResult("Điểm thưởng phải lớn hơn hoặc bằng 0");

        try
        {
            var exists = await _productDetailRepository.ExistsAsync(updateDto.Id);
            if (!exists)
                return new ErrorResponseResult("Không tìm thấy sản phẩm chi tiết");

            var productDetail = new ProductDetail
            {
                Id = updateDto.Id,
                SellingPrice = updateDto.SellingPrice,
                EarningPoints = updateDto.EarningPoints,
                SoldQuantity = updateDto.SoldQuantity,
                Description = updateDto.Description,
                IsActive = updateDto.IsActive ?? false,
                UpdatedDate = DateTime.UtcNow
            };

            await _productDetailRepository.UpdateAsync(productDetail);
            return new SuccessResponseResult("Cập nhật sản phẩm chi tiết thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> Remove(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID sản phẩm chi tiết phải lớn hơn 0");

        try
        {
            var exists = await _productDetailRepository.ExistsAsync(id);
            if (!exists)
                return new ErrorResponseResult("Không tìm thấy sản phẩm chi tiết");

            await _productDetailRepository.DeleteAsync(id);
            return new SuccessResponseResult("Xóa sản phẩm chi tiết thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> RemoveMultiple(ProductDetailDto.ListProductDetailRemoveDto removeDto)
    {
        if (removeDto.ProductDetailIds == null || removeDto.ProductDetailIds.Count == 0)
            return new ErrorResponseResult("Danh sách sản phẩm chi tiết không được rỗng");

        try
        {
            int deletedCount = 0;
            foreach (var id in removeDto.ProductDetailIds)
            {
                var deleted = await _productDetailRepository.DeleteAsync(id);
                if (deleted) deletedCount++;
            }

            return new SuccessResponseResult($"Xóa {deletedCount} sản phẩm chi tiết thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> RemainingProductQuantity(long productId)
    {
        if (productId <= 0)
            return new ErrorResponseResult("ID sản phẩm phải lớn hơn 0");

        try
        {
            var productDetails = await _productDetailRepository.GetProductDetailsByProductIdAsync(productId);
            if (productDetails == null || productDetails.Count == 0)
                return new ErrorResponseResult("Không tìm thấy chi tiết sản phẩm");

            var results = productDetails
                .Select(pd => ProductDetailMappings.RemainingProductQuantity(pd))
                .ToList();

            return new SuccessResponseResult(results);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }
}
