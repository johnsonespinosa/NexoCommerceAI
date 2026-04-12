using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICartRepository : IRepositoryAsync<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(TimeSpan abandonedSince, CancellationToken cancellationToken = default);
}