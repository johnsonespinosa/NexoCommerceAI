using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Specifications;

public sealed class CategoryByIdSpec : Specification<Category>
{
    public CategoryByIdSpec(Guid id)
    {
        Query.Where(c => c.Id == id && !c.IsDeleted);
    }
}