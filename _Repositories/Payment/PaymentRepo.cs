namespace TheLightStore.Repositories.Payment;

public class PaymentRepo : IPaymentRepo
{
    private readonly DBContext _context;

    public PaymentRepo(DBContext context)
    {
        _context = context;
    }

    public async Task<OrderPayment> GetByRequestIdAsync(Guid paymentRequestId)
    {
        return await _context.OrderPayments.FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);
    }

    public async Task<OrderPayment> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderPayments.FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task AddAsync(OrderPayment orderPayment)
    {
        orderPayment.CreatedAt = DateTime.UtcNow;
        await _context.OrderPayments.AddAsync(orderPayment);
    }

    public async Task UpdateAsync(OrderPayment orderPayment)
    {
        orderPayment.UpdatedAt = DateTime.UtcNow;
        _context.OrderPayments.Update(orderPayment);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}