namespace TheLightStore.Application.DTOs.GHN;

// Request Models
public class CreateShippingOrderRequest
{
    public int payment_type_id { get; set; } // 1: Người gửi thanh toán, 2: Người nhận thanh toán
    public string? note { get; set; }
    public string required_note { get; set; } = "KHONGCHOXEMHANG"; // CHOTHUHANG, CHOXEMHANGKHONGTHU, KHONGCHOXEMHANG
    public string? return_phone { get; set; }
    public string? return_address { get; set; }
    public string? return_district_name { get; set; }
    public string? return_ward_name { get; set; }
    public string? return_province_name { get; set; }
    public string? client_order_code { get; set; } // Mã đơn hàng riêng của khách hàng
    
    // From info (người gửi)
    public string? from_name { get; set; }
    public string? from_phone { get; set; }
    public string? from_address { get; set; }
    public string? from_ward_name { get; set; }
    public string? from_district_name { get; set; }
    public string? from_province_name { get; set; }
    
    // To info (người nhận) - Required
    public string to_name { get; set; } = null!;
    public string to_phone { get; set; } = null!;
    public string to_address { get; set; } = null!;
    public string to_ward_name { get; set; } = null!;
    public string to_district_name { get; set; } = null!;
    public string to_province_name { get; set; } = null!;
    
    // Address codes - Required by GHN API
    public string? to_ward_code { get; set; }
    public int? to_district_id { get; set; }
    public int? to_province_id { get; set; }
    
    // Order details
    public int cod_amount { get; set; } // Tiền thu hộ (tối đa 50,000,000)
    public string? content { get; set; }
    public int length { get; set; } // cm (tối đa 200)
    public int width { get; set; } // cm (tối đa 200) 
    public int height { get; set; } // cm (tối đa 200)
    public int weight { get; set; } // gram (tối đa 50,000)
    public int cod_failed_amount { get; set; }
    public int? pick_station_id { get; set; }
    public int? deliver_station_id { get; set; }
    public int insurance_value { get; set; } // Tối đa 5,000,000
    public int service_type_id { get; set; } = 2; // 2: Hàng nhẹ, 5: Hàng nặng
    public string? coupon { get; set; }
    public long? pickup_time { get; set; } // Unix timestamp
    public int[]? pick_shift { get; set; }
    public List<GHNOrderItem> items { get; set; } = new();
}

public class GHNOrderItem
{
    public string name { get; set; } = null!;
    public string? code { get; set; }
    public int quantity { get; set; }
    public int price { get; set; }
    public int length { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int weight { get; set; }
    public GHNItemCategory? category { get; set; }
}

public class GHNItemCategory
{
    public string? level1 { get; set; }
    public string? level2 { get; set; }
    public string? level3 { get; set; }
}

// Response Models
public class GHNApiResponse<T>
{
    public int code { get; set; }
    public string message { get; set; } = null!;
    public T? data { get; set; }
    public string? message_display { get; set; }
    public string? code_message { get; set; }
}

public class CreateShippingOrderResponse
{
    public string order_code { get; set; } = null!;
    public string sort_code { get; set; } = null!;
    public string trans_type { get; set; } = null!;
    public string? ward_encode { get; set; }
    public string? district_encode { get; set; }
    public GHNFee fee { get; set; } = null!;
    public int total_fee { get; set; } // 🔥 Changed from string to int
    public DateTime expected_delivery_time { get; set; }
}

public class GHNFee
{
    public int main_service { get; set; }
    public int insurance { get; set; }
    public int station_do { get; set; }
    public int station_pu { get; set; }
    public int @return { get; set; }
    public int r2s { get; set; }
    public int coupon { get; set; }
    public int cod_failed_fee { get; set; }
}

// 🚫 GHN Cancel Order DTOs
public class GHNCancelOrderRequest
{
    public List<string> order_codes { get; set; } = new();
}

public class GHNCancelOrderResponse
{
    public string order_code { get; set; } = null!;
    public bool result { get; set; }
    public string message { get; set; } = null!;
}