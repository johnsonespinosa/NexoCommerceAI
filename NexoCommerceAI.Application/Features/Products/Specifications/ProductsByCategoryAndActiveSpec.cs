using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ProductsByCategoryAndActiveSpec : Specification<Product>
{
    public ProductsByCategoryAndActiveSpec(Guid categoryId, bool onlyActive = true)
    {
        Query.Where(p => p.CategoryId == categoryId && !p.IsDeleted);
        
        if (onlyActive)
            Query.Where(p => p.IsActive);
            
        Query.Include(p => p.Category)
            .OrderBy(p => p.Name);
    }
}