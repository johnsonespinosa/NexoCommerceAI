using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public sealed class PaginatedProductsSpec : Specification<Product>
{
    public PaginatedProductsSpec(int pageNumber, int pageSize, string? searchTerm = null)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(p => p.Name.Contains(searchTerm) || 
                             p.Description.Contains(searchTerm));
        }
        
        Query.Where(p => !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}