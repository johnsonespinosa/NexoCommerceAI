using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Models;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasher,
    IJwtTokenService jwtService,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        // Obtener usuario por email
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }
        
        // Verificar si el usuario está activo
        if (!user.IsActive || user.IsDeleted)
        {
            logger.LogWarning("Login failed: User is inactive or deleted - {Email}", request.Email);
            throw new UnauthorizedException("Account is inactive. Please contact support.");
        }
        
        // Verificar contraseña
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }
        
        // Generar nuevo refresh token
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, refreshTokenExpiry);
        
        await userRepository.UpdateAsync(user, cancellationToken);
        
        // Generar token JWT
        var token = jwtService.GenerateToken(user);
        
        logger.LogInformation("User logged in successfully: {UserId} - {Email}", user.Id, user.Email);
        
        return new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            token,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            user.Role.Name
        );
    }
}