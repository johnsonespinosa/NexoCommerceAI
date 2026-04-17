using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.DTOs;

namespace NexoCommerceAI.Application.Products.Queries.GetProductAuditLogDetail;

internal sealed class GetProductAuditLogDetailQueryHandler(IAuditLogRepository repository) : IQueryHandler<GetProductAuditLogDetailQuery, AuditLogDto?>
{
    private readonly IAuditLogRepository _repository = repository;

    public async Task<Result<AuditLogDto?>> Handle(GetProductAuditLogDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return Result.Success(dto);
    }
}
