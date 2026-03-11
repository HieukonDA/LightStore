using TheLightStore.Application.DTOs.GHN;

using TheLightStore.Application.DTOs;
namespace TheLightStore.Application.Interfaces;
public interface IGHNService
{
    /// <summary>
    /// Tạo đơn hàng trên GHN
    /// </summary>
    /// <param name="orderId">ID đơn hàng trong hệ thống</param>
    /// <returns>Thông tin đơn hàng GHN được tạo</returns>
    Task<ServiceResult<CreateShippingOrderResponse>> CreateShippingOrderAsync(int orderId);
    
    /// <summary>
    /// Tạo đơn hàng GHN với thông tin tùy chỉnh
    /// </summary>
    /// <param name="request">Thông tin đơn hàng</param>
    /// <returns>Thông tin đơn hàng GHN được tạo</returns>
    Task<ServiceResult<CreateShippingOrderResponse>> CreateShippingOrderAsync(CreateShippingOrderRequest request);
    
    /// <summary>
    /// Lấy thông tin phí vận chuyển
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Thông tin phí vận chuyển</returns>
    Task<ServiceResult<GHNFee>> CalculateShippingFeeAsync(int orderId);
    
    /// <summary>
    /// Kiểm tra trạng thái đơn hàng trên GHN
    /// </summary>
    /// <param name="ghnOrderCode">Mã đơn hàng GHN</param>
    /// <returns>Trạng thái đơn hàng</returns>
    Task<ServiceResult<object>> GetOrderStatusAsync(string ghnOrderCode);
    
    /// <summary>
    /// Hủy đơn hàng trên GHN
    /// </summary>
    /// <param name="ghnOrderCode">Mã đơn hàng GHN</param>
    /// <returns>Kết quả hủy đơn</returns>
    Task<ServiceResult<bool>> CancelOrderAsync(string ghnOrderCode);
}