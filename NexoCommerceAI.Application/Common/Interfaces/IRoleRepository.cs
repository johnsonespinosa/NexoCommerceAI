using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IRoleRepository : IRepositoryAsync<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsRoleNameUniqueAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsRoleNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> HasUsersAssignedAsync(Guid roleId, CancellationToken cancellationToken = default);
}