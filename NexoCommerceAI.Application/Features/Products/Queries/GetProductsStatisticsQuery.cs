using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("products_statistics")]
public class GetProductsStatisticsQuery : IRequest<ProductsStatisticsResponse>
{
}