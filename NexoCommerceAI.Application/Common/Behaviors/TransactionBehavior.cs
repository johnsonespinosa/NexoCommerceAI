using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que envuelve la ejecución de comandos en una transacción de base de datos
/// </summary>
/// <typeparam name="TRequest">Tipo del request</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta</typeparam>
public class TransactionBehavior<TRequest, TResponse>(
    IApplicationDbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Solo aplicar transacción a comandos (no a queries)
        var isCommand = request.GetType().Name.EndsWith("Command");
        
        if (!isCommand)
        {
            return await next();
        }
        
        var strategy = dbContext.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            
            logger.LogInformation("Begin transaction for {RequestName}", typeof(TRequest).Name);
            
            try
            {
                var response = await next();
                
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                logger.LogInformation("Transaction committed for {RequestName}", typeof(TRequest).Name);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Transaction rolled back for {RequestName}", typeof(TRequest).Name);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}