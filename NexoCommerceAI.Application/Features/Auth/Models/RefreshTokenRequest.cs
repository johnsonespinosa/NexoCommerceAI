namespace NexoCommerceAI.Application.Features.Auth.Models;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);