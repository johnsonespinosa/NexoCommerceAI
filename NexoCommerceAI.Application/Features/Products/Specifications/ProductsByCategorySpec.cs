using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ProductsByCategorySpec : Specification<Product>
{
    public ProductsByCategorySpec(Guid categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
            .Include(p => p.Category)
            .OrderBy(p => p.Name);
    }
}