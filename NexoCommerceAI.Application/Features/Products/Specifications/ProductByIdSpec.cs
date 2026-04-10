using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(Guid id)
    {
        Query.Where(p => p.Id == id && !p.IsDeleted);
    }
}