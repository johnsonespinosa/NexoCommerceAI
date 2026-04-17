using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.DTOs;

namespace NexoCommerceAI.Application.Products.Queries.GetProductAuditLogs;

public sealed record GetProductAuditLogsQuery(
    string EntityType,
    string EntityId) : IQuery<IReadOnlyList<AuditLogDto>>;
