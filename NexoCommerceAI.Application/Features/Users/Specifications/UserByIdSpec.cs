using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UserByIdSpec : Specification<User>
{
    public UserByIdSpec(Guid id)
    {
        Query.Where(u => u.Id == id && !u.IsDeleted);
    }
}