namespace TheLightStore.Dtos.Search;

public class SearchFiltersDto
{
    public List<FilterOption> Categories { get; set; } = new();
    public List<FilterOption> Brands { get; set; } = new();
    public PriceRange PriceRange { get; set; } = new();
    public int TotalResults { get; set; }
}

public class OrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public int TotalItems { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class UserListDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class PopularSearchDto
{
    public string Query { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public int ResultCount { get; set; }
    public DateTime LastSearched { get; set; }
}

public class FilterOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; } // Số sản phẩm có filter này
    public bool IsSelected { get; set; } = false; // Để track user đã chọn chưa
}

public class PriceRange
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal CurrentMinPrice { get; set; } // User đang filter từ giá này
    public decimal CurrentMaxPrice { get; set; } // User đang filter đến giá này
}