using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.DTOs;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class RegisterUserHandler(
    IUserRepository repository,
    IRepositoryAsync<Role> roleRepository,
    IPasswordHasherService hasher,
    IJwtTokenService jwt,
    ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Validaciones de unicidad
        if (!await repository.IsEmailUniqueAsync(request.Email, cancellationToken))
            throw new ConflictException("Email already exists");

        if (!await repository.IsUserNameUniqueAsync(request.UserName, cancellationToken))
            throw new ConflictException("Username already exists");

        // Obtener role "Customer"
        var role = (await roleRepository.ListAsync(
            new RoleByNameSpec("Customer"), cancellationToken)).FirstOrDefault()
            ?? throw new Exception("Customer role not found");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = hasher.Hash(request.Password),
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Generar refresh token
        var refreshToken = jwt.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await repository.AddAsync(user, cancellationToken);

        // Generar JWT
        var token = jwt.GenerateToken(user);

        logger.LogInformation("User registered: {Email} ({UserId})", user.Email, user.Id);

        return new AuthResponse(user.Id, user.Email, user.UserName, token, refreshToken, DateTime.UtcNow.AddHours(1), user.Role.Name);
    }
}