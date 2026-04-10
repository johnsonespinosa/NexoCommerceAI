using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Specifications;

public sealed class ActiveRolesSpec : Specification<Role>
{
    public ActiveRolesSpec()
    {
        Query.Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Name);
    }
}