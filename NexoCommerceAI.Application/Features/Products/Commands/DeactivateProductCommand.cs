using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "low_stock_products", "product_by_id", "products_by_category")]
public class DeactivateProductCommand(Guid productId) : IRequest<bool>
{
    public Guid ProductId { get; init; } = productId;
}