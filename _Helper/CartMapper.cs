using TheLightStore.Models.Orders_Carts;
using TheLightStore.Dtos.Cart;

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
                CartItems = cart.CartItems.Select(ci => new CartItemDto
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
                    AddedAt = ci.AddedAt
                }).ToList()
            };
        }
    }
}
