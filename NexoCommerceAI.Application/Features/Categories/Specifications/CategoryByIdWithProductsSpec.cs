using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class CategoryByIdWithProductsSpec : Specification<Category>
{
    public CategoryByIdWithProductsSpec(Guid id)
    {
        Query.Where(c => c.Id == id && !c.IsDeleted)
            .Include(c => c.Products.Where(p => !p.IsDeleted && p.IsActive));
    }
}