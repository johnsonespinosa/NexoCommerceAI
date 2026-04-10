using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec()
    {
        Query.Where(product => product.IsActive && !product.IsDeleted);
    }
}