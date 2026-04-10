using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Application.Features.Roles.Queries;

namespace NexoCommerceAI.Application.Features.Roles.Handlers;

public class GetRolesListQueryHandler(
    IRoleRepository roleRepository,
    ILogger<GetRolesListQueryHandler> logger)
    : IRequestHandler<GetRolesListQuery, PaginatedResult<RoleResponse>>
{
    public async Task<PaginatedResult<RoleResponse>> Handle(GetRolesListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagination = request.Pagination;
            
            logger.LogInformation("Getting roles list - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}", 
                pagination.PageNumber, pagination.PageSize, pagination.SearchTerm ?? "none");
            
            var allRoles = await roleRepository.ListAsync(cancellationToken);
            var activeRoles = allRoles.Where(r => !r.IsDeleted).ToList();
            
            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
            {
                var searchTerm = pagination.SearchTerm.ToLower();
                activeRoles = activeRoles
                    .Where(r => r.Name.ToLower().Contains(searchTerm) || 
                                r.Description.ToLower().Contains(searchTerm))
                    .ToList();
            }
            
            var totalCount = activeRoles.Count;
            
            // Aplicar ordenamiento
            if (!string.IsNullOrWhiteSpace(pagination.SortBy))
            {
                var sortBy = pagination.SortBy.ToLower();
                if (sortBy == "name")
                {
                    activeRoles = pagination.SortDescending
                        ? activeRoles.OrderByDescending(r => r.Name).ToList()
                        : activeRoles.OrderBy(r => r.Name).ToList();
                }
                else
                {
                    activeRoles = pagination.SortDescending
                        ? activeRoles.OrderByDescending(r => r.CreatedAt).ToList()
                        : activeRoles.OrderBy(r => r.CreatedAt).ToList();
                }
            }
            else
            {
                activeRoles = activeRoles.OrderBy(r => r.Name).ToList();
            }
            
            // Aplicar paginación
            var items = activeRoles
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new RoleResponse(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsActive,
                    r.Users?.Count(u => !u.IsDeleted) ?? 0,
                    r.CreatedAt,
                    r.UpdatedAt))
                .ToList();
            
            logger.LogDebug("Found {TotalCount} roles, returning {ItemCount} items", totalCount, items.Count);
            
            return new PaginatedResult<RoleResponse>(
                items,
                totalCount,
                pagination.PageNumber,
                pagination.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting roles list");
            throw;
        }
    }
}