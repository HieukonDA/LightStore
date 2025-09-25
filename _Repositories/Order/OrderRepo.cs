using TheLightStore.Interfaces.Orders;

namespace TheLightStore.Repositories.Orders;

public class OrderRepo : IOrderRepo
{
    private readonly DBContext _context;

    public OrderRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Order>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        // Bắt đầu với query cơ bản
        var query = _context.Orders.AsQueryable()
            .Include(o => o.OrderPayments)
            .Include(o => o.OrderAddresses) 
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        // Áp dụng tìm kiếm nếu có
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(searchTerm) ||
                (o.CustomerName != null && o.CustomerName.ToLower().Contains(searchTerm)) ||
                (o.CustomerEmail != null && o.CustomerEmail.ToLower().Contains(searchTerm)) ||
                (o.CustomerPhone != null && o.CustomerPhone.ToLower().Contains(searchTerm)) ||
                o.OrderStatus.ToLower().Contains(searchTerm)
            );
        }

        // Áp dụng sắp xếp
        if (!string.IsNullOrWhiteSpace(request.Sort))
        {
            query = ApplySorting(query, request.Sort);
        }
        else
        {
            // Sắp xếp mặc định theo ngày tạo order (mới nhất trước)
            query = query.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue);
        }

        // Đếm tổng số records trước khi phân trang
        var totalCount = await query.CountAsync(ct);

        // Áp dụng phân trang
        var items = await query
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(ct);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.Size
        };
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderPayments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }


    // admin 
    /// <summary>
    /// Tổng doanh thu trong khoảng thời gian (nếu có)
    /// </summary>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();

        // Filter by date range if provided
        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        // Only count Delivered/Cancelled orders
        query = query.Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Cancelled);

        return await query.SumAsync(o => o.TotalAmount, ct);
    }

    public async Task<int> GetTotalOrdersCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();

        // Filter by date range if provided
        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        return await query.CountAsync(ct);
    }

    public async Task<decimal> GetAverageOrderValueAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();

        // Filter by date range if provided
        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        // Only calculate for completed/Cancelled orders
        query = query.Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Cancelled);

        var totalOrders = await query.CountAsync(ct);
        if (totalOrders == 0)
            return 0;

        var totalRevenue = await query.SumAsync(o => o.TotalAmount, ct);
        return totalRevenue / totalOrders;
    }

    public async Task<List<SalesDataPoint>> GetSalesByMonthAsync(int months = 6, CancellationToken ct = default)
    {
        var endDate = DateTime.Now;
        var startDate = endDate.AddMonths(-months);

        var salesData = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Cancelled)
            .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
            .Select(g => new SalesDataPoint
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Year = g.Key.Year,
                MonthNumber = g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.MonthNumber)
            .ToListAsync(ct);

        // Fill in missing months with zero values
        var result = new List<SalesDataPoint>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var existingData = salesData.FirstOrDefault(s => s.Year == currentDate.Year && s.MonthNumber == currentDate.Month);

            if (existingData != null)
            {
                result.Add(existingData);
            }
            else
            {
                result.Add(new SalesDataPoint
                {
                    Month = $"{currentDate.Year}-{currentDate.Month:D2}",
                    Year = currentDate.Year,
                    MonthNumber = currentDate.Month,
                    Revenue = 0,
                    OrderCount = 0
                });
            }

            currentDate = currentDate.AddMonths(1);
        }

        return result;
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int limit = 10, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User) // Assuming you have a User navigation property
            .OrderByDescending(o => o.OrderDate)
            .Take(limit)
            .ToListAsync(ct);
    }
    
    private IQueryable<Order> ApplySorting(IQueryable<Order> query, string sortField)
    {
        // Chuyển về lowercase để so sánh
        var field = sortField.ToLower();
        var isDescending = field.StartsWith("-");
        
        // Loại bỏ dấu "-" nếu có
        if (isDescending)
        {
            field = field.Substring(1);
        }

        return field switch
        {
            "id" => isDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id),
            "ordernumber" => isDescending ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber),
            "customername" => isDescending ? query.OrderByDescending(o => o.CustomerName) : query.OrderBy(o => o.CustomerName),
            "customeremail" => isDescending ? query.OrderByDescending(o => o.CustomerEmail) : query.OrderBy(o => o.CustomerEmail),
            "totalamount" => isDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            "orderstatus" => isDescending ? query.OrderByDescending(o => o.OrderStatus) : query.OrderBy(o => o.OrderStatus),
            "orderdate" => isDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
            "confirmedat" => isDescending ? query.OrderByDescending(o => o.ConfirmedAt) : query.OrderBy(o => o.ConfirmedAt),
            "shippedat" => isDescending ? query.OrderByDescending(o => o.ShippedAt) : query.OrderBy(o => o.ShippedAt),
            "deliveredat" => isDescending ? query.OrderByDescending(o => o.DeliveredAt) : query.OrderBy(o => o.DeliveredAt),
            _ => query.OrderByDescending(o => o.OrderDate ?? DateTime.MinValue) // Mặc định
        };
    }
}

public class OrderItemRepo : IOrderItemRepo
{
    private readonly DBContext _context;

    public OrderItemRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<OrderItem> items, CancellationToken ct = default)
    {
        await _context.OrderItems.AddRangeAsync(items, ct);
    }

    
}