using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Commands;

[InvalidateCache("categories_list", "category_by_id", "category_by_slug")]
public class CreateCategoryCommand : IRequest<CategoryResponse>
{
    public string? Name { get; init; } = default!;
    public string? Slug { get; init; }
}