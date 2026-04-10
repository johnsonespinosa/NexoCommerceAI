using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class ProductBySlugSpec : Specification<Product>
{
    public ProductBySlugSpec(string slug)
    {
        Query.Where(product => product.Slug == slug && !product.IsDeleted)
             .Include(product => product.Category);
    }
}