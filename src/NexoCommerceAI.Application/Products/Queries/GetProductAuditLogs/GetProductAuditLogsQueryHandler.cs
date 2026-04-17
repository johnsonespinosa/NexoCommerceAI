using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.DTOs;

namespace NexoCommerceAI.Application.Products.Queries.GetProductAuditLogs;

internal sealed class GetProductAuditLogsQueryHandler(IAuditLogRepository repository) : IQueryHandler<GetProductAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    private readonly IAuditLogRepository _repository = repository;

    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetProductAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetChangesForEntityAsync(request.EntityType, request.EntityId, cancellationToken);
        return Result.Success(list);
    }
}
