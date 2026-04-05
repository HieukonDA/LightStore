using System;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Mappings;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;

namespace TheLightStore.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;

    public BrandService(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<ResponseResult> GetAll(BrandDto.BrandFilterParams filterParams)
    {
        try
        {
            var result = await _brandRepository.GetAllAsync(filterParams);
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
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var result = await _brandRepository.GetByIdAsync(id);
        if (result == null)
            return new ErrorResponseResult("Không tìm thấy thương hiệu");

        return new SuccessResponseResult(result);
    }

    public async Task<ResponseResult> Create(BrandDto.BrandCreateDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.Name))
            return new ErrorResponseResult("Tên thương hiệu là bắt buộc");

        if (!string.IsNullOrWhiteSpace(createDto.Description))
        {
            var existingBrand = await _brandRepository.GetByCodeAsync(createDto.Description);
            if (existingBrand != null)
                return new ErrorResponseResult("Mã thương hiệu đã tồn tại");
        }

        var brand = BrandMappings.CreateDtoToEntity(createDto);
        brand.CreatedDate = DateTime.UtcNow;
        brand.IsActive = createDto.IsActive ?? true;

        var id = await _brandRepository.CreateAsync(brand);
        return new SuccessResponseResult(id);
    }

    public async Task<ResponseResult> Update(BrandDto.BrandUpdateDto updateDto)
    {
        if (updateDto.Id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var existingBrand = await _brandRepository.GetByIdAsync(updateDto.Id);
        if (existingBrand == null)
            return new ErrorResponseResult("Không tìm thấy thương hiệu");

        if (!string.IsNullOrWhiteSpace(updateDto.Description))
        {
            var brandWithSameCode = await _brandRepository.GetByCodeAsync(updateDto.Description);
            if (brandWithSameCode != null && brandWithSameCode.Id != updateDto.Id)
                return new ErrorResponseResult("Mã thương hiệu đã tồn tại");
        }

        var brand = await _brandRepository.GetByIdAsync(updateDto.Id);
        var updatedBrand = BrandMappings.UpdateDtoToEntity(updateDto, new Brand { Id = updateDto.Id });
        updatedBrand.UpdatedDate = DateTime.UtcNow;

        var result = await _brandRepository.UpdateAsync(updatedBrand);
        return result 
            ? new SuccessResponseResult("Cập nhật thương hiệu thành công")
            : new ErrorResponseResult("Cập nhật thương hiệu thất bại");
    }

    public async Task<ResponseResult> Remove(long id)
    {
        if (id <= 0)
            return new ErrorResponseResult("ID phải lớn hơn 0");

        var exists = await _brandRepository.ExistsAsync(id);
        if (!exists)
            return new ErrorResponseResult("Không tìm thấy thương hiệu");

        var result = await _brandRepository.DeleteAsync(id);
        return result 
            ? new SuccessResponseResult("Xóa thương hiệu thành công")
            : new ErrorResponseResult("Xóa thương hiệu thất bại");
    }
}
