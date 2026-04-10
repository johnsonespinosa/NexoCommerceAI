using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UserByIdWithRoleSpec : Specification<User>
{
    public UserByIdWithRoleSpec(Guid id)
    {
        Query.Where(u => u.Id == id && !u.IsDeleted)
            .Include(u => u.Role);
    }
}