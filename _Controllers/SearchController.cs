using Microsoft.AspNetCore.Mvc;
using TheLightStore.Dtos.Products;
using TheLightStore.Dtos.Paging;
using TheLightStore.Interfaces.Search;
using System.Threading.Tasks;

namespace TheLightStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo query
        /// </summary>
        [HttpPost("products")]
        public async Task<ActionResult<ServiceResult<PagedResult<ProductListDto>>>> SearchProducts([FromBody] SearchProductsRequest request)
        {
            var result = await _searchService.SearchProductsAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Lấy gợi ý sản phẩm theo từ khóa
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<ActionResult<ServiceResult<List<ProductSuggestionDto>>>> GetProductSuggestions([FromQuery] string query, [FromQuery] int limit = 10)
        {
            var result = await _searchService.GetProductSuggestionsAsync(query, limit);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Lấy bộ lọc tìm kiếm (categories, brands, price range)
        /// </summary>
        [HttpGet("filters")]
        public async Task<ActionResult<ServiceResult<SearchFiltersDto>>> GetProductFilters([FromQuery] string query)
        {
            var result = await _searchService.GetProductFiltersAsync(query);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
