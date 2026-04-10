using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class CategoryBySlugSpec : Specification<Category>
{
    public CategoryBySlugSpec(string slug)
    {
        Query.Where(c => c.Slug == slug && !c.IsDeleted);
    }
}