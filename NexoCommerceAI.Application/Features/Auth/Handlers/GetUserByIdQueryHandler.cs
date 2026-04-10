using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserByIdQueryHandler> logger)
    : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting user by ID: {UserId}", request.Id);
            
            var user = await userRepository.GetByIdWithRoleAsync(request.Id, cancellationToken);
            
            if (user == null || user.IsDeleted)
            {
                logger.LogWarning("User not found: {UserId}", request.Id);
                return null;
            }
            
            logger.LogDebug("User found: {UserId} - {Email}", user.Id, user.Email);
            
            return new UserResponse(
                user.Id,
                user.UserName,
                user.Email,
                user.Role.Name,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by ID: {UserId}", request.Id);
            throw;
        }
    }
}