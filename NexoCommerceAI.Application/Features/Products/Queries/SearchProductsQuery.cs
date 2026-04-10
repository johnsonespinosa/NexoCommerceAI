// Application/Features/Products/Queries/SearchProducts/SearchProductsQuery.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("search_products")]
public class SearchProductsQuery(string searchTerm, int take = 20) : IRequest<IReadOnlyList<ProductResponse>>
{
    public string SearchTerm { get; init; } = searchTerm;
    public int Take { get; init; } = take;
}