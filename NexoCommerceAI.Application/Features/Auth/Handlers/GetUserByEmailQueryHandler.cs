using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class GetUserByEmailQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserByEmailQueryHandler> logger)
    : IRequestHandler<GetUserByEmailQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting user by email: {Email}", request.Email);
            
            var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null || user.IsDeleted)
            {
                logger.LogWarning("User not found with email: {Email}", request.Email);
                return null;
            }
            
            // Cargar el rol si es necesario
            var userWithRole = await userRepository.GetByIdWithRoleAsync(user.Id, cancellationToken);
            
            logger.LogDebug("User found: {UserId} - {Email}", user.Id, user.Email);
            
            return new UserResponse(
                user.Id,
                user.UserName,
                user.Email,
                userWithRole?.Role.Name ?? "Unknown",
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by email: {Email}", request.Email);
            throw;
        }
    }
}