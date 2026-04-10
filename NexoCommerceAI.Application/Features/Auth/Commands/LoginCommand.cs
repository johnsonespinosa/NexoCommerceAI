using MediatR;
using NexoCommerceAI.Application.Features.Auth.Models;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
}