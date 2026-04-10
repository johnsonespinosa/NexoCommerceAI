using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Queries;

[Cacheable("category_by_id")]
public class GetCategoryByIdQuery(Guid id) : IRequest<CategoryResponse?>
{
    public Guid Id { get; init; } = id;
}