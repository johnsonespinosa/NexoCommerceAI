using MediatR;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class UpdateUserProfileCommand : IRequest<UserResponse>
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
}