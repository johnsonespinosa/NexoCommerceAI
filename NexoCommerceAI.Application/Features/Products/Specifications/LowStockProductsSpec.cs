using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class LowStockProductsSpec : Specification<Product>
{
    public LowStockProductsSpec(int threshold)
    {
        Query.Where(p => p.Stock <= threshold && !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Stock);
    }
}