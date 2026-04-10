using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    ILogger<DeleteRoleCommandHandler> logger)
    : IRequestHandler<DeleteRoleCommand, bool>
{
    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting role: {RoleId}", request.Id);
            
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException(nameof(Role), request.Id);
            
            // Verificar si es un rol predefinido
            if (IsPredefinedRole(role.Name))
                throw new ValidationException($"Cannot delete predefined role '{role.Name}'");
            
            // Verificar si tiene usuarios asignados
            var hasUsers = await roleRepository.HasUsersAssignedAsync(request.Id, cancellationToken);
            if (hasUsers)
                throw new ValidationException($"Cannot delete role '{role.Name}' because it has users assigned");
            
            if (role.IsDeleted)
            {
                logger.LogWarning("Role already deleted: {RoleId} - {RoleName}", role.Id, role.Name);
                return false;
            }
            
            role.SoftDelete();
            
            await roleRepository.UpdateAsync(role, cancellationToken);
            await roleRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role deleted successfully: {RoleId} - {RoleName}", role.Id, role.Name);
            
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting role: {RoleId}", request.Id);
            throw;
        }
    }
    
    private static bool IsPredefinedRole(string roleName)
    {
        return roleName is "Admin" or "Customer" or "Manager";
    }
}