using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class SearchProductSpec : Specification<Product>
{
    public SearchProductSpec(
        string? search,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isActive,
        int pageNumber,
        int pageSize)
    {
        Query.Where(product => product.IsDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            Query.Where(product => product.Name.Contains(normalized, StringComparison.CurrentCultureIgnoreCase) ||
                                   product.Description!.Contains(normalized, StringComparison.CurrentCultureIgnoreCase));
        }

        if (categoryId.HasValue)
        {
            Query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            Query.Where(product => product.Price >= minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            Query.Where(product => product.Price <= maxPrice.Value);
        }
        
        if(isActive.HasValue)
        {
            Query.Where(product => product.IsActive == isActive.Value);
        }
        
        Query.Include(product => product.Category)
            .OrderByDescending(product => product.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}