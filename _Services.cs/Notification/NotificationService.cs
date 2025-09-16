using TheLightStore.Interfaces.Email;
using TheLightStore.Interfaces.Notifications;
using TheLightStore.Models.Orders_Carts;

namespace TheLightStore.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task NotifyOrderCreatedAsync(Order order, CancellationToken ct = default)
    {
        var subject = $"[TheLightStore] Xác nhận đơn hàng {order.OrderNumber}";
        var body = $@"
Xin chào {order.CustomerName},

Cảm ơn bạn đã đặt hàng tại TheLightStore!
Mã đơn hàng của bạn là: {order.OrderNumber}.
Tổng thanh toán: {order.TotalAmount:C}.

Chúng tôi sẽ xử lý đơn hàng và thông báo khi có cập nhật.

Trân trọng,
TheLightStore
";

        if (!string.IsNullOrEmpty(order.CustomerEmail))
        {
            await _emailService.SendEmailAsync(order.CustomerEmail, subject, body);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(Order order, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        var subject = $"[TheLightStore] Cập nhật đơn hàng {order.OrderNumber}";
        var body = $@"
Xin chào {order.CustomerName},

Trạng thái đơn hàng {order.OrderNumber} đã thay đổi:
- Từ: {oldStatus}
- Sang: {newStatus}

Trân trọng,
TheLightStore
";

        if (!string.IsNullOrEmpty(order.CustomerEmail))
        {
            await _emailService.SendEmailAsync(order.CustomerEmail, subject, body);
        }
    }
}
