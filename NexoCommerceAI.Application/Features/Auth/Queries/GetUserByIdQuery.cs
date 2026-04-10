using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Queries;

[Cacheable("user_by_id")]
public class GetUserByIdQuery(Guid id) : IRequest<UserResponse?>
{
    public Guid Id { get; init; } = id;
}