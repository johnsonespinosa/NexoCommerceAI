using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class ProductBySkuSpec : Specification<Product>
{
    public ProductBySkuSpec(string sku)
    {
        Query.Where(product => product.Sku == sku);
    }
}