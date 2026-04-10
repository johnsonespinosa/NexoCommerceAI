using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Users.Specifications;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class UserRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<User>(dbContext), IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var spec = new UserByEmailSpec(email);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default)
    {
        var spec = new UserByUsernameSpec(username);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new UserByIdWithRoleSpec(id);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return await IsEmailUniqueAsync(email, null, cancellationToken);
    }
    
    public async Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        var spec = new UserByUsernameSpec(username);
        return !await AnyAsync(spec, cancellationToken);
    }
    
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var spec = new UserByRefreshTokenSpec(refreshToken);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.Where(u => u.Email == email && !u.IsDeleted);
    
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
    
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsUserNameUniqueAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.Where(u => u.UserName == username && !u.IsDeleted);
    
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
    
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var spec = new ActiveUsersSpec();
        return await ListAsync(spec, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var spec = new UsersByRoleSpec(roleId);
        return await ListAsync(spec, cancellationToken);
    }
}

