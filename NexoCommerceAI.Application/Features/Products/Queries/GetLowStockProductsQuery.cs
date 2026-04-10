using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("low_stock_products")]
public class GetLowStockProductsQuery(int threshold = 5) : IRequest<IReadOnlyList<ProductResponse>>
{
    public int Threshold { get; init; } = threshold;
}