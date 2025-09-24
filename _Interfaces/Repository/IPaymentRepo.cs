namespace TheLightStore.Interfaces.Repository;

public interface IPaymentRepo
{
    Task<OrderPayment> GetByRequestIdAsync(string paymentRequestId);
    Task<OrderPayment> GetByOrderIdAsync(int orderId);
    Task AddAsync(OrderPayment orderPayment);
    Task UpdateAsync(OrderPayment orderPayment);
    Task SaveChangesAsync();
}