namespace NexoCommerceAI.Application.Features.Auth.Models;

public record AuthResponse(
    Guid UserId,
    string Email,
    string UserName,
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    string Role
);