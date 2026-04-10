using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Application.Features.Roles.Queries;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class GetRoleByIdQueryHandler(
    IRoleRepository roleRepository,
    ILogger<GetRoleByIdQueryHandler> logger)
    : IRequestHandler<GetRoleByIdQuery, RoleResponse?>
{
    public async Task<RoleResponse?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting role by ID: {RoleId}", request.Id);
            
            var role = await roleRepository.GetByIdWithUsersAsync(request.Id, cancellationToken);
            
            if (role == null || role.IsDeleted)
            {
                logger.LogWarning("Role not found: {RoleId}", request.Id);
                return null;
            }
            
            var userCount = role.Users?.Count(u => !u.IsDeleted) ?? 0;
            
            logger.LogDebug("Role found: {RoleId} - {RoleName}, Users: {UserCount}", 
                role.Id, role.Name, userCount);
            
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
            logger.LogError(ex, "Error getting role by ID: {RoleId}", request.Id);
            throw;
        }
    }
}