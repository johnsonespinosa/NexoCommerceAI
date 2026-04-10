using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Specifications;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class RoleRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Role>(dbContext), IRoleRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var spec = new RoleByNameSpec(name);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<Role?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new RoleByIdWithUsersSpec(id);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<bool> IsRoleNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        var spec = new RoleByNameSpec(name);
        return !await AnyAsync(spec, cancellationToken);
    }
    
    public async Task<bool> IsRoleNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Roles.Where(r => r.Name == name && !r.IsDeleted);
    
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
    
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ActiveRolesSpec();
        return await ListAsync(spec, cancellationToken);
    }

    public async Task<bool> HasUsersAssignedAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var spec = new RoleByIdWithUsersSpec(roleId);
        var role = await FirstOrDefaultAsync(spec, cancellationToken);
        return role != null && role.Users.Any(u => !u.IsDeleted);
    }
}