using TheLightStore.Interfaces.Search;
using TheLightStore.Interfaces.Repository;
using TheLightStore.Dtos.Products;
using TheLightStore.Dtos.Paging;
using TheLightStore.Models.Products;

using Microsoft.Extensions.Logging;

namespace TheLightStore.Services.Search;

public class SearchService : ISearchService
{
    private readonly IProductRepo _productRepo;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IProductRepo productRepo, ILogger<SearchService> logger)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<ProductListDto>>> SearchProductsAsync(SearchProductsRequest request)
    {
        try
        {
            // Validate request
            var validationResult = ValidateSearchRequest(request);
            if (!validationResult.Success)
            {
                return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                    validationResult.Message,
                    validationResult.Errors
                );
            }

            // Call ProductRepo
            var products = await _productRepo.SearchAsync(request);

            // Map Product entities to ProductListDto
            var productDtos = products.Items.Select(MapToProductListDto).ToList();

            var pagedResult = new PagedResult<ProductListDto>
            {
                Items = productDtos,
                TotalCount = products.TotalCount,
                Page = products.Page,
                PageSize = products.PageSize
            };

            return ServiceResult<PagedResult<ProductListDto>>.SuccessResult(pagedResult, "Tìm kiếm sản phẩm thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching products with query: {Query}", request.Query);
            return ServiceResult<PagedResult<ProductListDto>>.FailureResult(
                "Có lỗi xảy ra khi tìm kiếm sản phẩm",
                new List<string> { ex.Message }
            );
        }
    }

    public async Task<ServiceResult<List<ProductSuggestionDto>>> GetProductSuggestionsAsync(string query, int limit = 10)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(query))
            {
                return ServiceResult<List<ProductSuggestionDto>>.SuccessResult(new List<ProductSuggestionDto>(), "Không có từ khóa tìm kiếm");
            }

            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }

            // Call ProductRepo
            var products = await _productRepo.GetProductSuggestionsAsync(query, limit);

            // Map to DTOs
            var suggestions = products.Select(p => new ProductSuggestionDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug, // Already exists in Product model
                Price = p.SalePrice, // Use SalePrice from actual model
                ImageUrl = p.ProductImages?.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl,
                CategoryName = p.Category?.Name ?? "",
                Brand = p.Brand?.Name
            }).ToList();

            return ServiceResult<List<ProductSuggestionDto>>.SuccessResult(suggestions, "Lấy gợi ý sản phẩm thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product suggestions for query: {Query}", query);
            return ServiceResult<List<ProductSuggestionDto>>.FailureResult(
                "Có lỗi xảy ra khi lấy gợi ý sản phẩm",
                new List<string> { ex.Message }
            );
        }
    }

    public async Task<ServiceResult<SearchFiltersDto>> GetProductFiltersAsync(string query)
    {
        try
        {
            // Call ProductRepo to get filter data
            var filtersDict = await _productRepo.GetSearchFiltersAsync(query);

            // Convert Dictionary to structured DTOs
            var categoryFilters = new List<FilterOption>();
            var brandFilters = new List<FilterOption>();
            var priceRange = new PriceRange();
            var totalResults = 0;

            foreach (var filter in filtersDict)
            {
                if (filter.Key.StartsWith("category_"))
                {
                    var categoryName = filter.Key.Replace("category_", "");
                    categoryFilters.Add(new FilterOption
                    {
                        Id = categoryName.GetHashCode(),
                        Name = categoryName,
                        Count = filter.Value,
                        IsSelected = false
                    });
                }
                else if (filter.Key.StartsWith("brand_"))
                {
                    var brandName = filter.Key.Replace("brand_", "");
                    brandFilters.Add(new FilterOption
                    {
                        Id = brandName.GetHashCode(),
                        Name = brandName,
                        Count = filter.Value,
                        IsSelected = false
                    });
                }
                else if (filter.Key == "min_price")
                {
                    priceRange.MinPrice = filter.Value;
                    priceRange.CurrentMinPrice = filter.Value;
                }
                else if (filter.Key == "max_price")
                {
                    priceRange.MaxPrice = filter.Value;
                    priceRange.CurrentMaxPrice = filter.Value;
                }
                else if (filter.Key == "total_products")
                {
                    totalResults = filter.Value;
                }
            }

            var searchFilters = new SearchFiltersDto
            {
                Categories = categoryFilters.OrderByDescending(c => c.Count).ToList(),
                Brands = brandFilters.OrderByDescending(b => b.Count).ToList(),
                PriceRange = priceRange,
                TotalResults = totalResults
            };

            return ServiceResult<SearchFiltersDto>.SuccessResult(searchFilters, "Lấy bộ lọc tìm kiếm thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting search filters for query: {Query}", query);
            return ServiceResult<SearchFiltersDto>.FailureResult(
                "Có lỗi xảy ra khi lấy bộ lọc tìm kiếm",
                new List<string> { ex.Message }
            );
        }
    }

    #region Private Helper Methods

    private ServiceResult<bool> ValidateSearchRequest(SearchProductsRequest request)
    {
        var errors = new List<string>();

        if (request == null)
        {
            return ServiceResult<bool>.FailureResult(
                "Yêu cầu tìm kiếm không được để trống",
                new List<string> { "Request is null" }
            );
        }

        if (request.Page <= 0)
        {
            errors.Add("Số trang phải lớn hơn 0");
        }

        if (request.Size <= 0 || request.Size > 100)
        {
            errors.Add("Kích thước trang phải từ 1 đến 100");
        }

        if (request.MinPrice.HasValue && request.MaxPrice.HasValue &&
            request.MinPrice.Value > request.MaxPrice.Value)
        {
            errors.Add("Giá tối thiểu không được lớn hơn giá tối đa");
        }

        if (errors.Any())
        {
            return ServiceResult<bool>.FailureResult("Dữ liệu đầu vào không hợp lệ", errors);
        }

        return ServiceResult<bool>.SuccessResult(true, "Validation passed");
    }

    private ProductListDto MapToProductListDto(Product product)
    {
        var discountPercentage = product.BasePrice > product.SalePrice
            ? Math.Round((product.BasePrice - product.SalePrice) / product.BasePrice * 100, 2)
            : 0m;

        var finalPrice = product.SalePrice;

        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Sku = product.Sku,

            // Pricing
            BasePrice = product.BasePrice,
            SalePrice = product.SalePrice,
            FinalPrice = finalPrice,
            IsOnSale = product.SalePrice < product.BasePrice,
            DiscountPercentage = discountPercentage,

            // Media
            ThumbnailUrl = product.ProductImages?.FirstOrDefault(i => i.IsPrimary ?? false)?.ImageUrl,

            // Stock & status
            IsInStock = product.ManageStock ? product.StockQuantity > 0 : true,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            IsNewProduct = (DateTime.Now - product.CreatedAt).TotalDays <= 30, // Ví dụ sản phẩm mới trong 30 ngày

            Rating = product.ProductReviews.Any()
            ? Math.Round((decimal)product.ProductReviews.Average(r => r.Rating), 2)
            : 0m,

            ReviewCount = product.ProductReviews.Count,

            // Related info
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "",
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,

            // Timestamps
            CreatedAt = product.CreatedAt
        };
    }


    #endregion
}