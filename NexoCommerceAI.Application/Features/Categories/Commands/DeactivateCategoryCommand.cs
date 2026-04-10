using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Categories.Commands;

[InvalidateCache("categories_list", "category_by_id", "category_by_slug")]
public class DeactivateCategoryCommand(Guid id) : IRequest<bool>
{
    public Guid Id { get; init; } = id;
}