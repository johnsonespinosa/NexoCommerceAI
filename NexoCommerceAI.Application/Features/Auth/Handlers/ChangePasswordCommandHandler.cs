using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasher,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing password for user: {UserId}", request.UserId);
        
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(User), request.UserId);
        
        // Verificar contraseña actual
        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect");
        
        // Hashear nueva contraseña
        var newPasswordHash = passwordHasher.Hash(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        
        // Revocar refresh token por seguridad
        user.RevokeRefreshToken();
        
        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
        
        return true;
    }
}