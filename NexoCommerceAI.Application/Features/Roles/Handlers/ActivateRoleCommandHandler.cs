using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class ActivateRoleCommandHandler(
    IRoleRepository roleRepository,
    ILogger<ActivateRoleCommandHandler> logger)
    : IRequestHandler<ActivateRoleCommand, bool>
{
    public async Task<bool> Handle(ActivateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Activating role: {RoleId}", request.Id);
            
            var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (role == null)
                throw new NotFoundException(nameof(Role), request.Id);
            
            if (role.IsDeleted)
                throw new ValidationException($"Cannot activate deleted role '{role.Name}'");
            
            if (role.IsActive)
            {
                logger.LogInformation("Role already active: {RoleId} - {RoleName}", role.Id, role.Name);
                return false;
            }
            
            role.Activate();
            
            await roleRepository.UpdateAsync(role, cancellationToken);
            await roleRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role activated successfully: {RoleId} - {RoleName}", role.Id, role.Name);
            
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
            logger.LogError(ex, "Error activating role: {RoleId}", request.Id);
            throw;
        }
    }
}