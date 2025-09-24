namespace TheLightStore.Interfaces.Search;

public interface ISearchService
{
    // Product Search
    Task<ServiceResult<PagedResult<ProductListDto>>> SearchProductsAsync(SearchProductsRequest request);
    Task<ServiceResult<List<ProductSuggestionDto>>> GetProductSuggestionsAsync(string query, int limit = 10);
    Task<ServiceResult<SearchFiltersDto>> GetProductFiltersAsync(string query);

    // Order Search (for Admin)
    // Task<ServiceResult<PagedResult<OrderListDto>>> SearchOrdersAsync(SearchOrdersRequest request);
    
    // // User Search (for Admin)
    // Task<ServiceResult<PagedResult<UserListDto>>> SearchUsersAsync(SearchUsersRequest request);
    
    // // Global Search (tìm tất cả)
    // Task<ServiceResult<GlobalSearchResultDto>> GlobalSearchAsync(string query, int limit = 20);
    
    // // Search Analytics
    // Task<ServiceResult<bool>> TrackSearchQueryAsync(string query, int? userId = null);
    // Task<ServiceResult<List<PopularSearchDto>>> GetPopularSearchesAsync(int limit = 10);
}