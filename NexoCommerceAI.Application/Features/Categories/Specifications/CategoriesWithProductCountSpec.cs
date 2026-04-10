using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class CategoriesWithProductCountSpec : Specification<Category>
{
    public CategoriesWithProductCountSpec()
    {
        Query.Where(c => !c.IsDeleted)
            .Include(c => c.Products.Where(p => !p.IsDeleted && p.IsActive));
    }
}