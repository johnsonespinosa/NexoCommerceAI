// Application/Features/Products/Queries/GetProductsByCategory/GetProductsByCategoryQuery.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("products_by_category")]
public class GetProductsByCategoryQuery(Guid categoryId, int? take = null) : IRequest<IReadOnlyList<ProductResponse>>
{
    public Guid CategoryId { get; init; } = categoryId;
    public int? Take { get; init; } = take;
}