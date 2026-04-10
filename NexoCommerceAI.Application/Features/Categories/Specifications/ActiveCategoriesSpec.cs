using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class ActiveCategoriesSpec : Specification<Category>
{
    public ActiveCategoriesSpec()
    {
        Query.Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name);
    }
}