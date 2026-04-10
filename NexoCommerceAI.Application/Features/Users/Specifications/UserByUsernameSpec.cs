using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UserByUsernameSpec : Specification<User>
{
    public UserByUsernameSpec(string username)
    {
        Query.Where(u => u.UserName == username && !u.IsDeleted);
    }
}