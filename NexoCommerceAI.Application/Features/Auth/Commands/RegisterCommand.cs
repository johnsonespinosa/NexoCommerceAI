using MediatR;
using NexoCommerceAI.Application.Features.Auth.DTOs;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public record RegisterCommand(string UserName, string Email, string Password, string ConfirmPassword) : IRequest<AuthResponse>;