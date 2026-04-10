using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    ILogger<UpdateRoleCommandHandler> logger)
    : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
    public async Task<RoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating role: {RoleId}", request.Id);
            
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException(nameof(Role), request.Id);
            
            // Verificar si es un rol predefinido
            if (IsPredefinedRole(role.Name) && request.Name != null && request.Name != role.Name)
                throw new ValidationException($"Cannot rename predefined role '{role.Name}'");
            
            role.Update(request.Name ?? role.Name, request.Description ?? role.Description);
            
            await roleRepository.UpdateAsync(role, cancellationToken);
            await roleRepository.SaveChangesAsync(cancellationToken);
            
            var userCount = role.Users?.Count(u => !u.IsDeleted) ?? 0;
            
            logger.LogInformation("Role updated successfully: {RoleId} - {RoleName}", role.Id, role.Name);
            
            return new RoleResponse(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                userCount,
                role.CreatedAt,
                role.UpdatedAt
            );
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid role data: {Message}", ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating role: {RoleId}", request.Id);
            throw;
        }
    }
    
    private static bool IsPredefinedRole(string roleName)
    {
        return roleName is "Admin" or "Customer" or "Manager";
    }
}