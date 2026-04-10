using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UserByEmailSpec : Specification<User>
{
    public UserByEmailSpec(string email)
    {
        Query.Where(u => u.Email == email && !u.IsDeleted);
    }
}