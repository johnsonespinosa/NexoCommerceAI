using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class LogoutCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            throw new UnauthorizedException("User not authenticated");
        
        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value, cancellationToken);
        
        if (user is null)
            throw new NotFoundException("User not found");
        
        // Usar el método de dominio para revocar el refresh token
        user.RevokeRefreshToken();
        
        await userRepository.UpdateAsync(user, cancellationToken);
        
        logger.LogInformation("User logged out: {Email}", user.Email);
        
        return Unit.Value;
    }
}