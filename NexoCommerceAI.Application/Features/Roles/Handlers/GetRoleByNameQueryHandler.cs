using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Application.Features.Roles.Queries;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class GetRoleByNameQueryHandler(
    IRoleRepository roleRepository,
    ILogger<GetRoleByNameQueryHandler> logger)
    : IRequestHandler<GetRoleByNameQuery, RoleResponse?>
{
    public async Task<RoleResponse?> Handle(GetRoleByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting role by name: {RoleName}", request.Name);
            
            var role = await roleRepository.GetByNameAsync(request.Name, cancellationToken);
            
            if (role == null || role.IsDeleted)
            {
                logger.LogWarning("Role not found: {RoleName}", request.Name);
                return null;
            }
            
            var userCount = await roleRepository.HasUsersAssignedAsync(role.Id, cancellationToken) 
                ? (role.Users?.Count(u => !u.IsDeleted) ?? 0) : 0;
            
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting role by name: {RoleName}", request.Name);
            throw;
        }
    }
}