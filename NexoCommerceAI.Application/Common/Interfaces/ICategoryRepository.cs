using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICategoryRepository : IRepositoryAsync<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(string? name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
}