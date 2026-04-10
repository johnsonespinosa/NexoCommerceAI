using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Models;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtService,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Refresh token attempt");
        
        // Validar el token expirado
        var principal = jwtService.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            throw new UnauthorizedException("Invalid token");
        
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new UnauthorizedException("Invalid token");
        
        // Obtener usuario por email
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            throw new UnauthorizedException("User not found");
        
        // Validar refresh token
        if (!user.IsRefreshTokenValid(request.RefreshToken))
            throw new UnauthorizedException("Invalid or expired refresh token");
        
        // Generar nuevos tokens
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(newRefreshToken, newRefreshTokenExpiry);
        
        await userRepository.UpdateAsync(user, cancellationToken);
        
        var newToken = jwtService.GenerateToken(user);
        
        logger.LogInformation("Token refreshed successfully for user: {UserId} - {Email}", user.Id, user.Email);
        
        return new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            newToken,
            newRefreshToken,
            DateTime.UtcNow.AddHours(1),
            user.Role.Name
        );
    }
}