using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ProductCountByCategorySpec : Specification<Product>
{
    public ProductCountByCategorySpec(Guid categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive);
    }
}