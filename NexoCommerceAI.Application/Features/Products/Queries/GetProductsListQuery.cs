using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("products_list")]
public class GetProductsListQuery : IRequest<PaginatedResult<ProductResponse>>
{
    public PaginationParams Pagination { get; init; } = new();
}