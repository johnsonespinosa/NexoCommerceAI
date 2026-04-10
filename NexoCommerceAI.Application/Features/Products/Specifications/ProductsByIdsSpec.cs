using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ProductsByIdsSpec : Specification<Product>
{
    public ProductsByIdsSpec(IEnumerable<Guid> ids)
    {
        Query.Where(p => ids.Contains(p.Id) && !p.IsDeleted)
            .Include(p => p.Category);
    }
}