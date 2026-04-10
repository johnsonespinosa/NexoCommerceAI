using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("products_on_sale")]
public class GetProductsOnSaleQuery(int? take = null) : IRequest<IReadOnlyList<ProductResponse>>
{
    public int? Take { get; init; } = take;
}