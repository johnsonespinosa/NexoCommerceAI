using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    ILogger<CreateRoleCommandHandler> logger)
    : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    public async Task<RoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating role: {RoleName}", request.Name);
            
            var role = Role.Create(request.Name, request.Description);
            
            await roleRepository.AddAsync(role, cancellationToken);
            await roleRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Role created successfully: {RoleId} - {RoleName}", role.Id, role.Name);
            
            return new RoleResponse(
                role.Id,
                role.Name,
                role.Description,
                role.IsActive,
                0,
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
            logger.LogError(ex, "Unexpected error creating role: {RoleName}", request.Name);
            throw;
        }
    }
}