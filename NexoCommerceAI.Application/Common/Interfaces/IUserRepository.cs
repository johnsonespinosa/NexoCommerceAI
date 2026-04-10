using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IUserRepository : IRepositoryAsync<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameUniqueAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameUniqueAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}