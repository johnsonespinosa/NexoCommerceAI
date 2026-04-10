using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Users.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Auth.Handlers;

public class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    ILogger<UpdateUserProfileCommandHandler> logger)
    : IRequestHandler<UpdateUserProfileCommand, UserResponse>
{
    public async Task<UserResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating profile for user: {UserId}", request.UserId);
        
        var user = await userRepository.GetByIdWithRoleAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException(nameof(User), request.UserId);
        
        // Verificar unicidad de email
        if (request.Email != user.Email && !await userRepository.IsEmailUniqueAsync(request.Email, request.UserId, cancellationToken))
            throw new ConflictException("Email already registered");
        
        // Verificar unicidad de username
        if (request.UserName != user.UserName && !await userRepository.IsUserNameUniqueAsync(request.UserName, request.UserId, cancellationToken))
            throw new ConflictException("Username already taken");
        
        user.UpdateProfile(request.UserName, request.Email);
        
        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Profile updated successfully for user: {UserId} - {Email}", user.Id, user.Email);
        
        return new UserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.Role?.Name ?? "Unknown",
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}