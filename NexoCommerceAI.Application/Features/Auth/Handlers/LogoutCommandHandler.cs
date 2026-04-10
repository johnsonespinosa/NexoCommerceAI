using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class LogoutCommandHandler(
    IUserRepository userRepository,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Logging out user: {UserId}", request.UserId);
            
            var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                logger.LogWarning("User not found during logout: {UserId}", request.UserId);
                throw new NotFoundException(nameof(User), request.UserId);
            }
            
            // Revocar el refresh token
            user.RevokeRefreshToken();
            
            await userRepository.UpdateAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("User logged out successfully: {UserId} - {Email}", user.Id, user.Email);
            
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout for user: {UserId}", request.UserId);
            throw;
        }
    }
}