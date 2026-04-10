using System.Security.Claims;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateRefreshToken(string refreshToken, string? storedRefreshToken, DateTime? expiryTime);
}