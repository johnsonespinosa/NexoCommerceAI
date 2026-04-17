using NexoCommerceAI.Application.DTOs;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLogDto>> GetChangesForEntityAsync(string entityType, string entityId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLogDto>> GetChangesByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLogDto>> GetChangesByUserAsync(string userId, CancellationToken ct = default);
    Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
