using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Queries;

[Cacheable("user_by_email")]
public class GetUserByEmailQuery(string email) : IRequest<UserResponse?>
{
    public string Email { get; init; } = email;
}