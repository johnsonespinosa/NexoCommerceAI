using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class CategoriesWithMinimumProductsSpec : Specification<Category>
{
    public CategoriesWithMinimumProductsSpec(int minProducts)
    {
        Query.Where(c => !c.IsDeleted && c.IsActive)
            .Include(c => c.Products.Where(p => !p.IsDeleted && p.IsActive))
            .Where(c => c.Products.Count >= minProducts)
            .OrderByDescending(c => c.Products.Count);
    }
}