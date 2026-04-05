using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IPromotionRepository _promotionRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IPromotionRepository promotionRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _promotionRepository = promotionRepository;
    }

    public async Task<ResponseResult> GetAll(ProductDto.ProductFilterParams filterParams)
    {
        try
        {
            var result = await _productRepository.GetAllAsync(filterParams);
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
            return new ErrorResponseResult("ID sản phẩm phải lớn hơn 0");

        try
        {
            var result = await _productRepository.GetByIdAsync(id);
            if (result == null)
                return new ErrorResponseResult("Không tìm thấy sản phẩm");

            return new SuccessResponseResult(result);
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(ex.Message);
        }
    }

    public async Task<ResponseResult> CreateByAdmin(ProductDto.ProductCreateDto createDto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(createDto.Name))
                return new ErrorResponseResult("Tên sản phẩm là bắt buộc");

            if (createDto.CategoryId <= 0)
                return new ErrorResponseResult("Danh mục sản phẩm là bắt buộc");

            // Auto-generate code if not provided
            if (string.IsNullOrEmpty(createDto.Code))
            {
                createDto.Code = await GenerateProductCodeAsync();
            }
            else
            {
                // Check code uniqueness
                var codeExists = await _productRepository.ExistsByCodeAsync(createDto.Code);
                if (codeExists)
                    return new ErrorResponseResult($"Mã sản phẩm '{createDto.Code}' đã tồn tại");
            }

            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
            if (category == null)
                return new ErrorResponseResult("Danh mục không tồn tại");

            // Validate brand if provided
            if (createDto.BrandId.HasValue && createDto.BrandId.Value > 0)
            {
                var brand = await _brandRepository.GetByIdAsync(createDto.BrandId.Value);
                if (brand == null)
                    return new ErrorResponseResult("Thương hiệu không tồn tại");
            }

            var product = new Product
            {
                Code = createDto.Code,
                ProductType = createDto.ProductType ?? "SELF_PRODUCED",
                Name = createDto.Name,
                CategoryId = createDto.CategoryId,
                BrandId = createDto.BrandId,
                IsInBusiness = createDto.IsInBusiness,
                IsOrderedOnline = createDto.IsOrderedOnline,
                IsPackaged = createDto.IsPackaged,
                Description = createDto.Description,
                Position = createDto.Position,
                ImageUrl = createDto.ImageUrl,
                IsActive = createDto.IsActive ?? true,
                CreatedDate = DateTime.UtcNow
            };

            await _productRepository.CreateAsync(product);
            return new SuccessResponseResult("Tạo sản phẩm thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Lỗi tạo sản phẩm: {ex.Message}");
        }
    }

    public async Task<ResponseResult> UpdateByAdmin(ProductDto.UpdateExtraDto updateDto)
    {
        try
        {
            if (updateDto.Id <= 0)
                return new ErrorResponseResult("ID sản phẩm không hợp lệ");

            if (string.IsNullOrEmpty(updateDto.Name))
                return new ErrorResponseResult("Tên sản phẩm là bắt buộc");

            if (updateDto.CategoryId <= 0)
                return new ErrorResponseResult("Danh mục sản phẩm là bắt buộc");

            // Get existing product
            var product = await _productRepository.GetProductWithDetailsAsync(updateDto.Id);
            if (product == null)
                return new ErrorResponseResult("Không tìm thấy sản phẩm");

            // Check code uniqueness
            if (product.Code != updateDto.Code)
            {
                var codeExists = await _productRepository.ExistsByCodeAsync(updateDto.Code, updateDto.Id);
                if (codeExists)
                    return new ErrorResponseResult($"Mã sản phẩm '{updateDto.Code}' đã tồn tại");
            }

            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId);
            if (category == null)
                return new ErrorResponseResult("Danh mục không tồn tại");

            // Validate brand if provided
            if (updateDto.BrandId.HasValue && updateDto.BrandId.Value > 0)
            {
                var brand = await _brandRepository.GetByIdAsync(updateDto.BrandId.Value);
                if (brand == null)
                    return new ErrorResponseResult("Thương hiệu không tồn tại");
            }

            // Update product properties
            product.Code = updateDto.Code;
            product.ProductType = updateDto.ProductType ?? "SELF_PRODUCED";
            product.Name = updateDto.Name;
            product.CategoryId = updateDto.CategoryId;
            product.BrandId = updateDto.BrandId;
            product.IsInBusiness = updateDto.IsInBusiness;
            product.IsOrderedOnline = updateDto.IsOrderedOnline;
            product.IsPackaged = updateDto.IsPackaged;
            product.Description = updateDto.Description;
            product.Position = updateDto.Position;
            product.ImageUrl = updateDto.ImageUrl;
            product.IsActive = updateDto.IsActive ?? product.IsActive;
            product.UpdatedDate = DateTime.UtcNow;

            // Update product details if provided
            if (updateDto.ProductDetails != null && updateDto.ProductDetails.Count > 0)
            {
                foreach (var detailDto in updateDto.ProductDetails)
                {
                    if (detailDto.Id <= 0)
                        return new ErrorResponseResult("Không thể tạo chi tiết sản phẩm mới khi cập nhật");

                    var existingDetail = product.ProductDetails.FirstOrDefault(pd => pd.Id == detailDto.Id);
                    if (existingDetail == null)
                        return new ErrorResponseResult($"Không tìm thấy chi tiết sản phẩm ID {detailDto.Id}");

                    existingDetail.SellingPrice = detailDto.SellingPrice;
                    existingDetail.EarningPoints = detailDto.EarningPoints;
                    existingDetail.SoldQuantity = detailDto.SoldQuantity;
                    existingDetail.Description = detailDto.Description;
                    existingDetail.IsActive = detailDto.IsActive ?? existingDetail.IsActive;
                    existingDetail.UpdatedDate = DateTime.UtcNow;
                }
            }

            await _productRepository.UpdateAsync(product);
            return new SuccessResponseResult("Cập nhật sản phẩm thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Lỗi cập nhật sản phẩm: {ex.Message}");
        }
    }

    public async Task<ResponseResult> UpdateByUserLogin(ProductDto.ProductUpdateDto updateDto)
    {
        try
        {
            if (updateDto.Id <= 0)
                return new ErrorResponseResult("ID sản phẩm không hợp lệ");

            var product = await _productRepository.GetProductWithDetailsAsync(updateDto.Id);
            if (product == null)
                return new ErrorResponseResult("Không tìm thấy sản phẩm");

            // User can only update basic description fields
            product.Description = updateDto.Description ?? product.Description;
            product.ImageUrl = updateDto.ImageUrl ?? product.ImageUrl;
            product.UpdatedDate = DateTime.UtcNow;

            // Update product details (update only, no create)
            if (updateDto.ProductDetails != null && updateDto.ProductDetails.Count > 0)
            {
                foreach (var detailDto in updateDto.ProductDetails)
                {
                    if (detailDto.Id > 0)
                    {
                        var existingDetail = product.ProductDetails.FirstOrDefault(pd => pd.Id == detailDto.Id);
                        if (existingDetail != null)
                        {
                            existingDetail.SellingPrice = detailDto.SellingPrice;
                            existingDetail.EarningPoints = detailDto.EarningPoints;
                            existingDetail.SoldQuantity = detailDto.SoldQuantity;
                            existingDetail.Description = detailDto.Description;
                            existingDetail.IsActive = detailDto.IsActive ?? existingDetail.IsActive;
                            existingDetail.UpdatedDate = DateTime.UtcNow;
                        }
                    }
                }
            }

            await _productRepository.UpdateAsync(product);
            return new SuccessResponseResult("Cập nhật sản phẩm thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Lỗi cập nhật sản phẩm: {ex.Message}");
        }
    }

    public async Task<ResponseResult> RemoveByAdmin(long id)
    {
        try
        {
            if (id <= 0)
                return new ErrorResponseResult("ID sản phẩm không hợp lệ");

            var product = await _productRepository.GetProductWithDetailsAsync(id);
            if (product == null)
                return new ErrorResponseResult("Không tìm thấy sản phẩm");

            // Check if product has details (simplified check)
            if (product.ProductDetails.Count > 0)
                return new ErrorResponseResult("Không thể xóa sản phẩm vì nó có chi tiết. Hãy cân nhắc đặt thành không hoạt động.");

            await _productRepository.DeleteAsync(id);
            return new SuccessResponseResult("Xóa sản phẩm thành công");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult($"Lỗi xóa sản phẩm: {ex.Message}");
        }
    }

    private async Task<string> GenerateProductCodeAsync()
    {
        // Simple code generation: PRD + timestamp
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var code = $"PRD{timestamp}";

        // Ensure uniqueness
        while (await _productRepository.ExistsByCodeAsync(code))
        {
            timestamp++;
            code = $"PRD{timestamp}";
        }

        return code;
    }
}
