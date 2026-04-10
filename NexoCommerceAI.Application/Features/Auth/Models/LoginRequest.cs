namespace NexoCommerceAI.Application.Features.Auth.Models;

public record LoginRequest(
    string Email,
    string Password
);