using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Categories.Commands;

[InvalidateCache("categories_list", "category_by_id", "category_by_slug")]
public record UpdateCategoryCommand(
    Guid Id,
    string? Name,
    string Slug) : IRequest<CategoryResponse>;