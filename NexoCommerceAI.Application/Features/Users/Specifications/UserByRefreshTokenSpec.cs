using Ardalis.Specification;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Users.Specifications;

public sealed class UserByRefreshTokenSpec : Specification<User>
{
    public UserByRefreshTokenSpec(string refreshToken)
    {
        Query.Where(u => u.RefreshToken == refreshToken && 
                         u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                         !u.IsDeleted);
    }
}