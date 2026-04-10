using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UsersByRoleSpec : Specification<User>
{
    public UsersByRoleSpec(Guid roleId)
    {
        Query.Where(u => u.RoleId == roleId && !u.IsDeleted && u.IsActive)
            .Include(u => u.Role)
            .OrderBy(u => u.UserName);
    }
}