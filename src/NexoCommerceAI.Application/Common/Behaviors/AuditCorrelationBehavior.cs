using MediatR;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Application.Common.Behaviors;

internal sealed class AuditCorrelationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public AuditCorrelationBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Ensure correlation is available via ICurrentUserService.GetCorrelationId
        // Nothing else to do here; behavior ensures the pipeline resolves current correlation early
        return await next();
    }
}
