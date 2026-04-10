using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("related_products")]
public class GetRelatedProductsQuery(Guid productId, int take = 5) : IRequest<IReadOnlyList<ProductResponse>>
{
    public Guid ProductId { get; init; } = productId;
    public int Take { get; init; } = take;
}