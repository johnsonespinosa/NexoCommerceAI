using MediatR;
using NexoCommerceAI.Application.Features.Auth.Models;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<AuthResponse>
{
    public string Token { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
}