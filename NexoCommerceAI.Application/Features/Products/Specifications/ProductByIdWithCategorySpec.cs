using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class ProductByIdWithCategorySpec : Specification<Product>
{
    public ProductByIdWithCategorySpec(Guid id)
    {
        Query.Where(product => product.Id == id && !product.IsDeleted)
             .Include(product => product.Category);
    }
}