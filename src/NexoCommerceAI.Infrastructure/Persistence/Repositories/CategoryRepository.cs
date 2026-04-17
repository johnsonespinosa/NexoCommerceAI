using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == id, cancellationToken);
    }
}
