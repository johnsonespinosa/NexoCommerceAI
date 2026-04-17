using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NexoCommerceAI.Infrastructure.Persistence;

internal sealed class AuditEntry
{
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public EntityEntry Entry { get; set; } = null!;

    public string ToJson(object obj) => JsonSerializer.Serialize(obj);
}
