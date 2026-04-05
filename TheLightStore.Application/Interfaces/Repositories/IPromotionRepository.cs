using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Products;
using static TheLightStore.Application.Dtos.PromotionDto;

namespace TheLightStore.Application.Interfaces.Repositories;

public interface IPromotionRepository
{
    Task<PaginationModel<PromotionGetDto>> GetAllAsync(PromotionFilterParams filterParams);
    Task<PromotionGetDto?> GetByIdAsync(long id);
    Task<Promotion?> GetByCodeAsync(string code);
    Task<bool> ExistsAsync(long id);
    Task<long> CreateAsync(Promotion promotion);
    Task<bool> UpdateAsync(Promotion promotion);
    Task<bool> DeleteAsync(long id);
    Task<List<PromotionGetDto>> GetAllActiveAsync();
}
