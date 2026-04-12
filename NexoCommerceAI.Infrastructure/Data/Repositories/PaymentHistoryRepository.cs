using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class PaymentHistoryRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<PaymentHistory>(dbContext), IPaymentHistoryRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<PaymentHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}