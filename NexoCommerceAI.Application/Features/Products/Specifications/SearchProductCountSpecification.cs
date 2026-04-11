using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Specifications;

public class SearchProductCountSpecification : Specification<Product>
{
    public SearchProductCountSpecification(
        string? search,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isActive)
    {
        Query.Where(product => product.IsDeleted == false);
        
        if(!string.IsNullOrWhiteSpace(search))
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
    }
}