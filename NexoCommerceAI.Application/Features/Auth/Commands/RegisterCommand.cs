using MediatR;
using NexoCommerceAI.Application.Features.Auth.Models;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<AuthResponse>
{
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string ConfirmPassword { get; init; } = default!;
}