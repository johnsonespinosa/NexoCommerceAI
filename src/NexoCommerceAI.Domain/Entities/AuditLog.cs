using System.ComponentModel.DataAnnotations;

namespace NexoCommerceAI.Domain.Entities;

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Restore
}

public sealed class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Full name or simple name of the entity
    public string EntityType { get; set; } = string.Empty;

    // The primary key of the entity as string
    public string EntityId { get; set; } = string.Empty;

    public AuditAction Action { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public Guid? CorrelationId { get; set; }

    // Factory methods
    public static AuditLog CreateForCreate(string entityType, string entityId, string? newValues, string changedBy, DateTime changedAt, string? ip = null, string? userAgent = null, Guid? correlationId = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Create,
            NewValues = newValues,
            ChangedBy = changedBy,
            ChangedAt = changedAt,
            IpAddress = ip,
            UserAgent = userAgent,
            CorrelationId = correlationId
        };
    }

    public static AuditLog CreateForUpdate(string entityType, string entityId, string? oldValues, string? newValues, string changedBy, DateTime changedAt, string? ip = null, string? userAgent = null, Guid? correlationId = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Update,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedBy = changedBy,
            ChangedAt = changedAt,
            IpAddress = ip,
            UserAgent = userAgent,
            CorrelationId = correlationId
        };
    }

    public static AuditLog CreateForDelete(string entityType, string entityId, string? oldValues, string changedBy, DateTime changedAt, string? ip = null, string? userAgent = null, Guid? correlationId = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Delete,
            OldValues = oldValues,
            ChangedBy = changedBy,
            ChangedAt = changedAt,
            IpAddress = ip,
            UserAgent = userAgent,
            CorrelationId = correlationId
        };
    }

    public static AuditLog CreateForRestore(string entityType, string entityId, string? newValues, string changedBy, DateTime changedAt, string? ip = null, string? userAgent = null, Guid? correlationId = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Restore,
            NewValues = newValues,
            ChangedBy = changedBy,
            ChangedAt = changedAt,
            IpAddress = ip,
            UserAgent = userAgent,
            CorrelationId = correlationId
        };
    }
}
