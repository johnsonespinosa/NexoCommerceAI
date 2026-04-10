using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasherService passwordHasher,
    IJwtTokenService jwtService,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Registering new user: {Email}", request.Email);
        
        // Verificar si el email ya existe
        if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
            throw new ConflictException("Email already registered");
        
        // Verificar si el username ya existe
        if (!await userRepository.IsUserNameUniqueAsync(request.UserName, cancellationToken))
            throw new ConflictException("Username already taken");
        
        // Obtener el rol por defecto (Customer)
        var defaultRole = await roleRepository.GetByNameAsync("Customer", cancellationToken);
        if (defaultRole is null)
            throw new NotFoundException("Default role 'Customer' not found. Please contact support.");
        
        // Hashear la contraseña
        var passwordHash = passwordHasher.Hash(request.Password);
        
        // Crear el usuario
        var user = User.Create(request.UserName, request.Email, passwordHash, defaultRole.Id);
        
        await userRepository.AddAsync(user, cancellationToken);
        
        // Generar refresh token
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, refreshTokenExpiry);
        
        await userRepository.UpdateAsync(user, cancellationToken);
        
        // Generar token JWT
        var token = jwtService.GenerateToken(user);
        
        logger.LogInformation("User registered successfully: {UserId} - {Email}", user.Id, user.Email);
        
        return new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            token,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            defaultRole.Name
        );
    }
}