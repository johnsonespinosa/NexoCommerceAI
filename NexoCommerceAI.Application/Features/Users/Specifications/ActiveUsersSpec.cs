using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class ActiveUsersSpec : Specification<User>
{
    public ActiveUsersSpec()
    {
        Query.Where(u => u.IsActive && !u.IsDeleted)
            .Include(u => u.Role)
            .OrderBy(u => u.UserName);
    }
}