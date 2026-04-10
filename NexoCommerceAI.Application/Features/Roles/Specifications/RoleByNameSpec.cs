using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Specifications;

public sealed class RoleByNameSpec : Specification<Role>
{
    public RoleByNameSpec(string name)
    {
        Query.Where(r => r.Name == name && !r.IsDeleted);
    }
}