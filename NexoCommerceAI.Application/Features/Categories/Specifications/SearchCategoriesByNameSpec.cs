using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class SearchCategoriesByNameSpec : Specification<Category>
{
    public SearchCategoriesByNameSpec(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(c => c.Name.Contains(searchTerm) && !c.IsDeleted && c.IsActive);
        }
        else
        {
            Query.Where(c => !c.IsDeleted && c.IsActive);
        }
        
        Query.OrderBy(c => c.Name);
    }
}