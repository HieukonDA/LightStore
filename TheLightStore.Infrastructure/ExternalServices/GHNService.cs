using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheLightStore.Application.DTOs;
using TheLightStore.Application.DTOs.GHN;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Services;
using TheLightStore.Domain.Entities.Orders;
using TheLightStore.Infrastructure.Configuration;

namespace TheLightStore.Infrastructure.ExternalServices;

public class GHNService : IGHNService
{
    private readonly ILogger<GHNService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOrderRepo _orderRepo;
    private readonly GHNSettings _ghnSettings;

    public GHNService(
        ILogger<GHNService> logger, 
        HttpClient httpClient, 
        IOrderRepo orderRepo,
        IOptions<GHNSettings> ghnSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _orderRepo = orderRepo;
        _ghnSettings = ghnSettings.Value;
        
        // Setup HTTP client headers
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("ShopId", _ghnSettings.ShopId);
        _httpClient.DefaultRequestHeaders.Add("Token", _ghnSettings.Token);
        // Note: Content-Type sẽ được set trong StringContent, không được set ở DefaultRequestHeaders
        
        _logger.LogInformation("🚛 GHN Service initialized - Environment: {Environment}", 
            _ghnSettings.IsTestMode ? "TEST" : "PRODUCTION");
    }

    public async Task<ServiceResult<CreateShippingOrderResponse>> CreateShippingOrderAsync(int orderId)
    {
        try
        {
            _logger.LogInformation("🚛 GHN: Creating shipping order for OrderId: {OrderId}", orderId);
            
            // Lấy thông tin đơn hàng từ database
            var order = await _orderRepo.GetByIdAsync(orderId); // TODO: Implement GetOrderWithDetailsAsync
            if (order == null)
            {
                _logger.LogWarning("❌ GHN: Order not found - OrderId: {OrderId}", orderId);
                return ServiceResult<CreateShippingOrderResponse>.FailureResult("Order not found", new List<string> { "Invalid order ID" });
            }

            // Chuyển đổi Order sang GHN request
            var ghnRequest = await MapOrderToGHNRequest(order);
            
            // Gọi API tạo đơn GHN
            return await CreateShippingOrderAsync(ghnRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GHN ERROR: Failed to create shipping order for OrderId: {OrderId}", orderId);
            return ServiceResult<CreateShippingOrderResponse>.FailureResult("Failed to create GHN order", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<CreateShippingOrderResponse>> CreateShippingOrderAsync(CreateShippingOrderRequest request)
    {
        try
        {
            _logger.LogInformation("🚛 GHN: Creating shipping order - ClientOrderCode: {ClientOrderCode}, ToName: {ToName}", 
                request.client_order_code, request.to_name);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var apiUrl = _ghnSettings.IsTestMode 
                ? "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create"
                : "https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create";

            _logger.LogInformation("🚛 GHN: Calling API - {Url}", apiUrl);
            _logger.LogDebug("🚛 GHN Request: {Json}", json);

            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("🚛 GHN Response: StatusCode={StatusCode}, Content={Content}", 
                response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ GHN API Error: StatusCode={StatusCode}, Content={Content}", 
                    response.StatusCode, responseContent);
                return ServiceResult<CreateShippingOrderResponse>.FailureResult("GHN API call failed", new List<string> { responseContent });
            }

            var ghnResponse = JsonSerializer.Deserialize<GHNApiResponse<CreateShippingOrderResponse>>(responseContent, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (ghnResponse?.code != 200 || ghnResponse.data == null)
            {
                _logger.LogError("❌ GHN API returned error: Code={Code}, Message={Message}", 
                    ghnResponse?.code, ghnResponse?.message);
                return ServiceResult<CreateShippingOrderResponse>.FailureResult(
                    ghnResponse?.message ?? "Unknown GHN error", 
                    new List<string> { ghnResponse?.code_message ?? "GHN_ERROR" });
            }

            _logger.LogInformation("✅ GHN: Successfully created order - GHN OrderCode: {OrderCode}, TotalFee: {TotalFee}", 
                ghnResponse.data.order_code, ghnResponse.data.total_fee);

            return ServiceResult<CreateShippingOrderResponse>.SuccessResult(ghnResponse.data, "GHN order created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GHN ERROR: Exception during API call");
            return ServiceResult<CreateShippingOrderResponse>.FailureResult("GHN service error", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<GHNFee>> CalculateShippingFeeAsync(int orderId)
    {
        // TODO: Implement calculate shipping fee
        await Task.Delay(1);
        return ServiceResult<GHNFee>.FailureResult("Not implemented yet", new List<string> { "Method not implemented" });
    }

    public async Task<ServiceResult<object>> GetOrderStatusAsync(string ghnOrderCode)
    {
        // TODO: Implement get order status
        await Task.Delay(1);
        return ServiceResult<object>.FailureResult("Not implemented yet", new List<string> { "Method not implemented" });
    }

    public async Task<ServiceResult<bool>> CancelOrderAsync(string ghnOrderCode)
    {
        try
        {
            _logger.LogInformation("🚫 GHN: Canceling order with GHN OrderCode: {GHNOrderCode}", ghnOrderCode);
            
            if (string.IsNullOrEmpty(ghnOrderCode))
            {
                _logger.LogWarning("❌ GHN: GHN OrderCode is required for cancellation");
                return ServiceResult<bool>.FailureResult("GHN OrderCode is required", new List<string> { "Invalid GHN order code" });
            }

            // Tạo cancel request
            var cancelRequest = new GHNCancelOrderRequest
            {
                order_codes = new List<string> { ghnOrderCode }
            };

            var jsonContent = JsonSerializer.Serialize(cancelRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Determine API endpoint based on environment
            var baseUrl = _ghnSettings.IsTestMode 
                ? "https://dev-online-gateway.ghn.vn" 
                : "https://online-gateway.ghn.vn";
            var apiUrl = $"{baseUrl}/shiip/public-api/v2/switch-status/cancel";

            _logger.LogInformation("🚫 GHN: Calling cancel API - {ApiUrl}", apiUrl);

            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("🚫 GHN Cancel Response: StatusCode={StatusCode}, Content={Content}", 
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var ghnResponse = JsonSerializer.Deserialize<GHNApiResponse<List<GHNCancelOrderResponse>>>(responseContent);
                
                if (ghnResponse?.code == 200 && ghnResponse.data?.Any() == true)
                {
                    var cancelResult = ghnResponse.data.First();
                    if (cancelResult.result)
                    {
                        _logger.LogInformation("✅ GHN: Successfully canceled order - {GHNOrderCode}", ghnOrderCode);
                        return ServiceResult<bool>.SuccessResult(true, $"Successfully canceled GHN order {ghnOrderCode}");
                    }
                    else
                    {
                        _logger.LogWarning("❌ GHN: Failed to cancel order - {GHNOrderCode}, Message: {Message}", 
                            ghnOrderCode, cancelResult.message);
                        return ServiceResult<bool>.FailureResult($"GHN cancel failed: {cancelResult.message}", 
                            new List<string> { cancelResult.message });
                    }
                }
                else
                {
                    _logger.LogWarning("❌ GHN: Invalid response format or failed - {Response}", responseContent);
                    return ServiceResult<bool>.FailureResult("Invalid GHN response format", 
                        new List<string> { ghnResponse?.message ?? "Unknown error" });
                }
            }
            else
            {
                _logger.LogError("❌ GHN: HTTP request failed - StatusCode: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                return ServiceResult<bool>.FailureResult("GHN API request failed", 
                    new List<string> { $"HTTP {response.StatusCode}: {responseContent}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ GHN: Exception occurred while canceling order {GHNOrderCode}", ghnOrderCode);
            return ServiceResult<bool>.FailureResult("Failed to cancel GHN order", new List<string> { ex.Message });
        }
    }

    private async Task<CreateShippingOrderRequest> MapOrderToGHNRequest(Order order)
    {
        _logger.LogInformation("🔄 GHN: Mapping Order {OrderId} to GHN request", order.Id);
        
        // Debug shipping address
        _logger.LogInformation("🚚 GHN DEBUG: ShippingAddress null = {IsNull}", order.ShippingAddress == null);
        _logger.LogInformation("🚚 GHN DEBUG: OrderAddresses count = {Count}", order.OrderAddresses?.Count ?? 0);
        if (order.OrderAddresses?.Any() == true)
        {
            foreach (var addr in order.OrderAddresses)
            {
                _logger.LogInformation("🚚 GHN DEBUG: Address Type='{Type}', Phone='{Phone}', Name='{Name}'", 
                    addr.AddressType, addr.Phone, addr.RecipientName);
            }
        }

        // Tính toán kích thước và trọng lượng từ OrderItems
        var totalWeight = 0;
        var totalLength = 0;
        var totalWidth = 0;
        var totalHeight = 0;
        var items = new List<GHNOrderItem>();

        foreach (var item in order.OrderItems ?? new List<OrderItem>())
        {
            // Giả sử có thông tin size/weight trong Product hoặc ProductVariant
            var itemWeight = 500; // gram - default weight per item
            var itemLength = 20;  // cm - default dimensions
            var itemWidth = 15;
            var itemHeight = 10;

            // TODO: Get actual dimensions from Product/Variant
            // if (item.Variant != null) { ... }
            // else if (item.Product != null) { ... }

            totalWeight += itemWeight * item.Quantity;
            totalLength = Math.Max(totalLength, itemLength);
            totalWidth = Math.Max(totalWidth, itemWidth); 
            totalHeight += itemHeight * item.Quantity;

            items.Add(new GHNOrderItem
            {
                name = item.Product?.Name ?? "Product",
                code = item.Product?.Id.ToString() ?? item.ProductId.ToString(),
                quantity = item.Quantity,
                price = (int)item.UnitPrice,
                length = itemLength,
                width = itemWidth,
                height = itemHeight,
                weight = itemWeight,
                category = new GHNItemCategory { level1 = "Thời trang" }
            });
        }

        // Determine phone number with fallback
        var toPhone = order.ShippingAddress?.Phone ?? order.User?.Phone ?? order.CustomerPhone ?? "";
        _logger.LogInformation("🚚 GHN DEBUG: Final to_phone = '{Phone}' (from ShippingAddress={SA}, User={U}, Customer={C})", 
            toPhone, order.ShippingAddress?.Phone, order.User?.Phone, order.CustomerPhone);
        
        // Map address to GHN codes
        var addressMapping = GetGHNAddressMapping(
            order.ShippingAddress?.City ?? "", 
            order.ShippingAddress?.District ?? "", 
            order.ShippingAddress?.Ward ?? ""
        );
        
        var ghnRequest = new CreateShippingOrderRequest
        {
            client_order_code = order.Id.ToString(),
            payment_type_id = order.PaymentMethod?.ToLower() == "cod" ? 2 : 1, // COD: người nhận trả, Online: người gửi trả
            required_note = "KHONGCHOXEMHANG", // Không cho xem hàng
            note = $"Đơn hàng #{order.Id} - {order.Notes}",
            
            // Thông tin người gửi (shop) - sẽ lấy từ config
            from_name = _ghnSettings.ShopName,
            from_phone = _ghnSettings.ShopPhone,
            from_address = _ghnSettings.ShopAddress,
            from_ward_name = _ghnSettings.ShopWardName,
            from_district_name = _ghnSettings.ShopDistrictName,
            from_province_name = _ghnSettings.ShopProvinceName,
            
            // Thông tin người nhận
            to_name = order.ShippingAddress?.RecipientName ?? "Khách hàng",
            to_phone = toPhone,
            to_address = $"{order.ShippingAddress?.AddressLine1} {order.ShippingAddress?.AddressLine2}".Trim(),
            to_ward_name = order.ShippingAddress?.Ward ?? "",
            to_district_name = order.ShippingAddress?.District ?? "",
            to_province_name = order.ShippingAddress?.Province ?? order.ShippingAddress?.City ?? "",
            
            // 🔥 GHN Address Codes - REQUIRED
            to_province_id = addressMapping.ProvinceId,
            to_district_id = addressMapping.DistrictId,
            to_ward_code = addressMapping.WardCode,
            
            // Thông tin đơn hàng
            cod_amount = order.PaymentMethod?.ToLower() == "cod" ? (int)order.TotalAmount : 0,
            content = $"Đơn hàng TheLightStore #{order.Id}",
            
            // Kích thước và trọng lượng
            length = Math.Min(totalLength, 200), // Max 200cm
            width = Math.Min(totalWidth, 200),
            height = Math.Min(totalHeight, 200),
            weight = Math.Min(totalWeight, 50000), // Max 50kg
            
            service_type_id = 2, // Hàng nhẹ
            insurance_value = Math.Min((int)order.TotalAmount, 5000000), // Max 5M
            items = items
        };
        
        _logger.LogInformation("🚚 GHN: Generated request with to_phone='{Phone}', to_name='{Name}', ward_code='{WardCode}'", 
            ghnRequest.to_phone, ghnRequest.to_name, ghnRequest.to_ward_code);
            
        return ghnRequest;
    }
    
    /// <summary>
    /// Map địa chỉ Việt Nam sang mã GHN
    /// </summary>
    private GHNAddressMapping GetGHNAddressMapping(string city, string district, string ward)
    {
        _logger.LogInformation("🗺️ GHN: Mapping address - City: '{City}', District: '{District}', Ward: '{Ward}'", 
            city, district, ward);
        
        // Normalize input
        var normalizedCity = city.Trim().ToLower();
        var normalizedDistrict = district.Trim().ToLower();
        var normalizedWard = ward.Trim().ToLower();
        
        // TP.HCM mappings
        if (normalizedCity.Contains("hồ chí minh") || normalizedCity.Contains("hcm") || normalizedCity.Contains("tp.hcm"))
        {
            // Quận Gò Vấp
            if (normalizedDistrict.Contains("gò vấp"))
            {
                if (normalizedWard.Contains("phường 10")) return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1444, WardCode = "21010" };
                if (normalizedWard.Contains("phường 1")) return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1444, WardCode = "21001" };
                if (normalizedWard.Contains("phường 11")) return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1444, WardCode = "21011" };
                // Default Gò Vấp
                return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1444, WardCode = "21010" };
            }
            
            // Quận 1
            if (normalizedDistrict.Contains("quận 1") || normalizedDistrict == "1")
            {
                if (normalizedWard.Contains("phường bến nghé")) return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1463, WardCode = "20101" };
                if (normalizedWard.Contains("phường bến thành")) return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1463, WardCode = "20102" };
                // Default Quận 1
                return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1463, WardCode = "20101" };
            }
            
            // Quận 3
            if (normalizedDistrict.Contains("quận 3") || normalizedDistrict == "3")
            {
                return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1465, WardCode = "20301" };
            }
            
            // Default TP.HCM - Quận 1
            return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1463, WardCode = "20101" };
        }
        
        // Hà Nội mappings
        if (normalizedCity.Contains("hà nội") || normalizedCity.Contains("hanoi"))
        {
            // Quận Hoàn Kiếm
            if (normalizedDistrict.Contains("hoàn kiếm"))
            {
                return new GHNAddressMapping { ProvinceId = 201, DistrictId = 1442, WardCode = "20201" };
            }
            
            // Default Hà Nội
            return new GHNAddressMapping { ProvinceId = 201, DistrictId = 1442, WardCode = "20201" };
        }
        
        _logger.LogWarning("⚠️ GHN: No mapping found for address, using default TP.HCM");
        // Default fallback - TP.HCM, Quận 1, Phường Bến Nghé
        return new GHNAddressMapping { ProvinceId = 202, DistrictId = 1463, WardCode = "20101" };
    }
}
