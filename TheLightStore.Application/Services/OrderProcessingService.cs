using Microsoft.Extensions.Logging;
using TheLightStore.Application.DTOs;
using TheLightStore.Application.DTOs.GHN;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Entities.Orders;
// Temporary - using old interface until migration
using IOrderRepo = TheLightStore.Application.Interfaces.Repositories.IOrderRepo;

namespace TheLightStore.Application.Services;

public class OrderProcessingService
{
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly IOrderRepo _orderRepo;
    private readonly IGHNService _ghnService;

    public OrderProcessingService(
        ILogger<OrderProcessingService> logger,
        IOrderRepo orderRepo,
        IGHNService ghnService)
    {
        _logger = logger;
        _orderRepo = orderRepo;
        _ghnService = ghnService;
    }

    /// <summary>
    /// Xử lý đơn hàng sau khi được tạo hoặc thanh toán
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="isPaymentCompleted">Có phải thanh toán đã hoàn tất không</param>
    /// <returns>Kết quả xử lý</returns>
    public async Task<ServiceResult<bool>> ProcessOrderAsync(int orderId, bool isPaymentCompleted = false)
    {
        try
        {
            _logger.LogInformation("📦 ORDER PROCESSING: Starting process for OrderId: {OrderId}, PaymentCompleted: {PaymentCompleted}", 
                orderId, isPaymentCompleted);

            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("❌ ORDER: Order not found - OrderId: {OrderId}", orderId);
                return ServiceResult<bool>.FailureResult("Order not found", new List<string> { "Invalid order ID" });
            }

            _logger.LogInformation("📦 ORDER: Processing order {OrderNumber} - PaymentMethod: {PaymentMethod}, Status: {Status}", 
                order.OrderNumber, order.PaymentMethod, order.OrderStatus);

            // Phân luồng xử lý theo phương thức thanh toán
            var shouldCreateGHNOrder = ShouldCreateGHNOrder(order, isPaymentCompleted);
            
            if (shouldCreateGHNOrder)
            {
                _logger.LogInformation("🚛 ORDER: Creating GHN shipping order for OrderId: {OrderId}", orderId);
                
                var ghnResult = await _ghnService.CreateShippingOrderAsync(orderId);
                if (ghnResult.Success && ghnResult.Data != null)
                {
                    // Cập nhật thông tin GHN vào đơn hàng
                    await UpdateOrderWithGHNInfo(order, ghnResult.Data);
                    
                    _logger.LogInformation("✅ ORDER: Successfully created GHN order - OrderId: {OrderId}, GHNOrderCode: {GHNOrderCode}", 
                        orderId, ghnResult.Data.order_code);
                }
                else
                {
                    _logger.LogError("❌ ORDER: Failed to create GHN order - OrderId: {OrderId}, Errors: {@Errors}", 
                        orderId, ghnResult.Errors);
                    
                    // Không fail toàn bộ process nếu GHN tạo lỗi, chỉ log error
                }
            }
            else
            {
                _logger.LogInformation("⏳ ORDER: Waiting for payment completion - OrderId: {OrderId}, PaymentMethod: {PaymentMethod}", 
                    orderId, order.PaymentMethod);
            }

            return ServiceResult<bool>.SuccessResult(true, "Order processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ORDER PROCESSING ERROR: Failed to process OrderId: {OrderId}", orderId);
            return ServiceResult<bool>.FailureResult("Order processing failed", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Xác định có nên tạo đơn GHN không
    /// </summary>
    /// <param name="order">Đơn hàng</param>
    /// <param name="isPaymentCompleted">Thanh toán đã hoàn tất chưa</param>
    /// <returns>True nếu nên tạo đơn GHN</returns>
    private static bool ShouldCreateGHNOrder(Order order, bool isPaymentCompleted)
    {
        // Đã có đơn GHN rồi thì không tạo nữa
        if (!string.IsNullOrEmpty(order.GHNOrderCode))
        {
            return false;
        }

        // COD: Tạo đơn GHN ngay
        if (order.PaymentMethod?.ToLower() == "cod")
        {
            return true;
        }

        // Online payment: Phải đợi thanh toán xong
        if (order.PaymentMethod?.ToLower() == "momo")
        {
            return isPaymentCompleted;
        }

        // Default: không tạo
        return false;
    }

    /// <summary>
    /// Cập nhật thông tin GHN vào đơn hàng
    /// </summary>
    /// <param name="order">Đơn hàng</param>
    /// <param name="ghnResponse">Kết quả từ GHN</param>
    private async Task UpdateOrderWithGHNInfo(Order order, CreateShippingOrderResponse ghnResponse)
    {
        try
        {
            order.GHNOrderCode = ghnResponse.order_code;
            order.GHNSortCode = ghnResponse.sort_code;
            order.GHNTransType = ghnResponse.trans_type;
            order.GHNTotalFee = (decimal)ghnResponse.total_fee;
            order.GHNExpectedDelivery = ghnResponse.expected_delivery_time;
            order.GHNStatus = "Created";
            order.GHNCreatedAt = DateTime.UtcNow;

            // Cập nhật phí vận chuyển nếu chưa có
            if (order.ShippingCost == null || order.ShippingCost == 0)
            {
                order.ShippingCost = (decimal)ghnResponse.total_fee;
                order.TotalAmount += order.ShippingCost.Value;
            }

            await _orderRepo.UpdateAsync(order);
            
            _logger.LogInformation("✅ ORDER: Updated order with GHN info - OrderId: {OrderId}, GHNOrderCode: {GHNOrderCode}, ShippingFee: {ShippingFee}", 
                order.Id, order.GHNOrderCode, order.ShippingCost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ORDER: Failed to update order with GHN info - OrderId: {OrderId}", order.Id);
            throw;
        }
    }

    /// <summary>
    /// Xử lý khi thanh toán hoàn tất
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Kết quả xử lý</returns>
    public async Task<ServiceResult<bool>> HandlePaymentCompletedAsync(int orderId)
    {
        _logger.LogInformation("💳 PAYMENT: Processing payment completion for OrderId: {OrderId}", orderId);
        
        // Xử lý đơn hàng với flag thanh toán hoàn tất
        return await ProcessOrderAsync(orderId, isPaymentCompleted: true);
    }
}
