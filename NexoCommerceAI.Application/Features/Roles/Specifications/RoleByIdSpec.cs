using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Specifications;

public sealed class RoleByIdSpec : Specification<Role>
{
    public RoleByIdSpec(Guid id)
    {
        Query.Where(r => r.Id == id && !r.IsDeleted);
    }
}