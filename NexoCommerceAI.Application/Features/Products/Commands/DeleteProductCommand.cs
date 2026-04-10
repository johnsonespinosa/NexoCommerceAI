using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "product_by_id")]
public class DeleteProductCommand(Guid id) : IRequest<bool>
{
    public Guid Id { get; init; } = id;
}