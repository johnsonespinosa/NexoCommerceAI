// Application/Features/Auth/Queries/GetCurrentUser/GetCurrentUserQuery.cs

using MediatR;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Queries;

public class GetCurrentUserQuery(Guid userId) : IRequest<UserResponse?>
{
    public Guid UserId { get; init; } = userId;
}