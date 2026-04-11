using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Specifications;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class CategoryRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Category>(dbContext), ICategoryRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var spec = new CategoryBySlugSpec(slug);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await IsSlugUniqueAsync(slug, null, cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categories.Where(c => c.Slug == slug && !c.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string? name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categories.Where(c => c.Name == name && !c.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new CategoryByIdSpec(id);
        return await AnyAsync(spec, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}