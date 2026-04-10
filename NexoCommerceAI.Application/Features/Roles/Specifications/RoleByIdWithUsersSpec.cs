using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Specifications;

public sealed class RoleByIdWithUsersSpec : Specification<Role>
{
    public RoleByIdWithUsersSpec(Guid id)
    {
        Query.Where(r => r.Id == id && !r.IsDeleted)
            .Include(r => r.Users.Where(u => !u.IsDeleted));
    }
}