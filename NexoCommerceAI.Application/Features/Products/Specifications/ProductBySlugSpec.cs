using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class ProductBySlugSpecification : Specification<Product>
{
    public ProductBySlugSpecification(string slug)
    {
        Query.Where(product => product.Slug == slug && !product.IsDeleted)
             .Include(product => product.Category);
    }
}