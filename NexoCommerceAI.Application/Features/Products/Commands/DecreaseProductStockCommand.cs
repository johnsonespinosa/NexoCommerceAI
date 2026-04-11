using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "low_stock_products", "product_by_id")]
public class DecreaseProductStockCommand(Guid productId, int quantity) : IRequest<bool>
{
    public Guid ProductId { get; init; } = productId;
    public int Quantity { get; init; } = quantity;
}