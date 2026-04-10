using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("featured_products")]
public class GetFeaturedProductsQuery(int take = 10) : IRequest<IReadOnlyList<ProductResponse>>
{
    public int Take { get; init; } = take;
}