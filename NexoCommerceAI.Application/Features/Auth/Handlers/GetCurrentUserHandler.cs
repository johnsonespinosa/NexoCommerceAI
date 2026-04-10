using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ILogger<GetCurrentUserQueryHandler> logger)
    : IRequestHandler<GetCurrentUserQuery, UserResponse?>
{
    public async Task<UserResponse?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting current user: {UserId}", request.UserId);
        
        var user = await userRepository.GetByIdWithRoleAsync(request.UserId, cancellationToken);
        
        if (user == null || user.IsDeleted)
        {
            logger.LogWarning("User not found: {UserId}", request.UserId);
            return null;
        }
        
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
}