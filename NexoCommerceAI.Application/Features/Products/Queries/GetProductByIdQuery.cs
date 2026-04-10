using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("product_by_id")]
public class GetProductByIdQuery(Guid productId) : IRequest<ProductResponse?>
{
    public Guid Id { get; init; } = productId;
}