using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Queries;

[Cacheable("category_by_slug")]
public class GetCategoryBySlugQuery(string slug) : IRequest<CategoryResponse?>
{
    public string Slug { get; init; } = slug;
}