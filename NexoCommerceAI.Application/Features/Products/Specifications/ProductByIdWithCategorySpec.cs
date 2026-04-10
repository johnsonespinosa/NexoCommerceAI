using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class ProductByIdWithCategorySpecification : Specification<Product>
{
    public ProductByIdWithCategorySpecification(Guid id)
    {
        Query.Where(product => product.Id == id && !product.IsDeleted)
             .Include(product => product.Category);
    }
}