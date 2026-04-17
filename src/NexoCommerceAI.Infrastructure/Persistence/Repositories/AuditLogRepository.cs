using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.DTOs;

namespace NexoCommerceAI.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<AuditLogDto>> GetChangesForEntityAsync(string entityType, string entityId, CancellationToken ct = default)
    {
        return await _dbContext.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.ChangedAt)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CorrelationId = a.CorrelationId,
                Action = a.Action.ToString()
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetChangesByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _dbContext.AuditLogs
            .Where(a => a.ChangedAt >= start && a.ChangedAt <= end)
            .OrderByDescending(a => a.ChangedAt)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CorrelationId = a.CorrelationId,
                Action = a.Action.ToString()
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetChangesByUserAsync(string userId, CancellationToken ct = default)
    {
        return await _dbContext.AuditLogs
            .Where(a => a.ChangedBy == userId)
            .OrderByDescending(a => a.ChangedAt)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CorrelationId = a.CorrelationId,
                Action = a.Action.ToString()
            })
            .ToListAsync(ct);
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _dbContext.AuditLogs.FindAsync([id], ct);
        if (a == null) return null;
        return new AuditLogDto
        {
            Id = a.Id,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            ChangedBy = a.ChangedBy,
            ChangedAt = a.ChangedAt,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            CorrelationId = a.CorrelationId,
            Action = a.Action.ToString()
        };
    }
}
