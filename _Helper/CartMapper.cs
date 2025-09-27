using TheLightStore.Models.Orders_Carts;
using TheLightStore.Dtos.Cart;
using TheLightStore.Dtos.Orders;

namespace TheLightStore.Helpers
{
    public static class CartMapper
    {
        public static CartDto MapToDto(ShoppingCart cart)
        {
            if (cart == null) return null!;

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                ItemsCount = cart.ItemsCount,
                Subtotal = cart.Subtotal,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt ?? DateTime.UtcNow,
                CartItems = cart.CartItems.Select(ci => {
                    // ✅ Lấy stock từ Variant hoặc Product
                    var stock = ci.Variant?.StockQuantity ?? ci.Product?.StockQuantity ?? 0;
                    var maxStock = ci.Product?.StockQuantity ?? 0;
                    
                    return new CartItemDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product?.Name ?? string.Empty,
                        ProductSku = ci.Product?.Sku ?? string.Empty,
                        ProductImageUrl = ci.Product?.ProductImages.FirstOrDefault()?.ImageUrl ?? string.Empty, // ánh xạ từ Product

                        VariantId = ci.VariantId,
                        VariantName = ci.Variant?.Name, // ánh xạ từ Variant

                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        OriginalPrice = ci.Product?.SalePrice < ci.Product?.BasePrice ? ci.Product?.BasePrice : null,
                        TotalPrice = ci.Quantity * ci.UnitPrice,

                        IsAvailable = ci.Product?.IsActive ?? false, // check sản phẩm còn active không
                        AddedAt = ci.AddedAt,
                        
                        // ✅ Thêm thông tin stock
                        Stock = stock,
                        MaxStock = maxStock
                    };
                }).ToList()
            };
        }

        public static OrderDto MapOrderToDto(Order order)
        {
            if (order == null) return null!;

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate ?? DateTime.UtcNow,

                Subtotal = order.Subtotal,
                TaxAmount = order.TaxAmount,
                ShippingCost = order.ShippingCost,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,

                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone,

                // ✅ Shipping Address (nếu có)
                ShippingAddress = order.OrderAddresses
                    .Where(a => a.AddressType == "shipping")
                    .Select(a => new OrderAddressDto
                    {
                        AddressType = a.AddressType,
                        RecipientName = a.RecipientName,
                        Phone = a.Phone,
                        AddressLine1 = a.AddressLine1,
                        AddressLine2 = a.AddressLine2,
                        Ward = a.Ward,
                        District = a.District,
                        City = a.City,
                        Province = a.Province,
                        PostalCode = a.PostalCode
                    })
                    .FirstOrDefault() ?? new OrderAddressDto(),

                // ✅ Items
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductName,
                    ProductSku = oi.ProductSku,
                    VariantName = oi.VariantName,
                    ProductAttributes = oi.ProductAttributes,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),

                // ✅ Payment (lấy thanh toán gần nhất, thường chỉ 1 bản ghi)
                Payment = order.OrderPayments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new OrderPaymentDto
                    {
                        Id = p.Id,
                        OrderId = p.OrderId,
                        PaymentMethod = p.PaymentMethod,
                        PaymentStatus = p.PaymentStatus,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        TransactionId = p.TransactionId,
                        PaymentRequestId = p.PaymentRequestId,
                        PaidAt = p.PaidAt,
                        FailedAt = p.FailedAt,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefault() ?? new OrderPaymentDto()
            };
        }
    }
}
