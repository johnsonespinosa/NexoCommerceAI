using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Queries;

[Cacheable("categories_list")]
public class GetCategoriesListQuery : IRequest<PaginatedResult<CategoryResponse>>
{
    public PaginationParams Pagination { get; init; } = new();
}