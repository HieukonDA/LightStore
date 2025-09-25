using TheLightStore.Interfaces.Orders;
using TheLightStore.Dtos.Orders;

namespace TheLightStore.Controllers.Dashboard;

[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IAuthService _authService;
    private readonly IOrderService _orderService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ICategoryService categoryService,
        IAuthService authService,
        IOrderService orderService,
        ILogger<DashboardController> logger)
    {
        _categoryService = categoryService;
        _authService = authService;
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get category statistics for dashboard pie chart
    /// </summary>
    /// <returns>Category statistics with product counts and percentages</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<CategoryStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategoryStats()
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching category statistics");

            var result = await _categoryService.GetCategoryStatsAsync();

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch category statistics: {Error}", result.Message);
                return StatusCode(500, new
                {
                    message = result.Message,
                    errors = result.Errors
                });
            }

            // Transform data to match frontend chart format
            var chartData = new
            {
                labels = result.Data.Select(x => x.CategoryName).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        data = result.Data.Select(x => x.ProductCount).ToArray(),
                        backgroundColor = new[] { "#0891b2", "#06b6d4", "#14b8a6", "#0d9488", "#059669", "#047857" }
                    }
                }
            };

            return Ok(chartData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCategoryStats endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }


    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _authService.GetTotalCustomersCountAsync(fromDate, toDate);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    
    /// <summary>
    /// Get total revenue for dashboard
    /// </summary>
    /// <param name="fromDate">Start date (optional)</param>
    /// <param name="toDate">End date (optional)</param>
    /// <returns>Total revenue amount</returns>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ServiceResult<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching total revenue from {FromDate} to {ToDate}", fromDate, toDate);

            var result = await _orderService.GetTotalRevenueAsync(fromDate, toDate);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch total revenue: {Error}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Dashboard: Total revenue retrieved successfully: {Revenue:C}", result.Data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetTotalRevenue endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get total orders count for dashboard
    /// </summary>
    /// <param name="fromDate">Start date (optional)</param>
    /// <param name="toDate">End date (optional)</param>
    /// <returns>Total orders count</returns>
    [HttpGet("orders-count")]
    [ProducesResponseType(typeof(ServiceResult<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTotalOrdersCount([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching total orders count from {FromDate} to {ToDate}", fromDate, toDate);

            var result = await _orderService.GetTotalOrdersCountAsync(fromDate, toDate);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch total orders count: {Error}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Dashboard: Total orders count retrieved successfully: {Count}", result.Data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetTotalOrdersCount endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get average order value for dashboard
    /// </summary>
    /// <param name="fromDate">Start date (optional)</param>
    /// <param name="toDate">End date (optional)</param>
    /// <returns>Average order value</returns>
    [HttpGet("average-order-value")]
    [ProducesResponseType(typeof(ServiceResult<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAverageOrderValue([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching average order value from {FromDate} to {ToDate}", fromDate, toDate);

            var result = await _orderService.GetAverageOrderValueAsync(fromDate, toDate);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch average order value: {Error}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Dashboard: Average order value retrieved successfully: {AOV:C}", result.Data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAverageOrderValue endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get sales data by month for dashboard charts
    /// </summary>
    /// <param name="months">Number of months to retrieve (default: 6)</param>
    /// <returns>Sales data points for chart visualization</returns>
    [HttpGet("sales-by-month")]
    [ProducesResponseType(typeof(ServiceResult<List<SalesDataPoint>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesByMonth([FromQuery] int months = 6)
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching sales data for {Months} months", months);

            var result = await _orderService.GetSalesByMonthAsync(months);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch sales data: {Error}", result.Message);
                return BadRequest(result);
            }

            // Transform data for chart.js format
            var chartData = new
            {
                labels = result.Data.Select(x => x.Month).ToArray(),
                datasets = new[]
                {
                    new
                    {
                        label = "Sales",
                        data = result.Data.Select(x => x.Revenue).ToArray(),
                        borderColor = "#0891b2",
                        backgroundColor = "rgba(8, 145, 178, 0.1)",
                        fill = true
                    }
                }
            };

            _logger.LogInformation("Dashboard: Sales data retrieved successfully for {Count} months", result.Data?.Count ?? 0);
            return Ok(new { success = true, data = chartData, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetSalesByMonth endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get recent orders for dashboard
    /// </summary>
    /// <param name="limit">Number of recent orders to retrieve (default: 10)</param>
    /// <returns>List of recent orders as DTOs</returns>
    [HttpGet("recent-orders")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("Dashboard: Fetching {Limit} recent orders", limit);

            var result = await _orderService.GetRecentOrdersAsync(limit);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to fetch recent orders: {Error}", result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Dashboard: Recent orders retrieved successfully: {Count} orders", result.Data?.Count() ?? 0);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetRecentOrders endpoint");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

}