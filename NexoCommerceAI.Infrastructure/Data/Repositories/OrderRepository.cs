using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class OrderRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Order>(dbContext), IOrderRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }
    
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);
    }
    
    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.UserId == userId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.Status == status && !o.IsDeleted)
            .OrderBy(o => o.CreatedAt)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .CountAsync(o => o.UserId == userId && !o.IsDeleted, cancellationToken);
    }
    
    public async Task<IReadOnlyList<Order>> GetPendingOrdersAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.Status == OrderStatus.Pending && 
                        o.CreatedAt < olderThan && 
                        !o.IsDeleted)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        // Buscar orden a través de PaymentHistory o metadata
        var paymentHistory = await _dbContext.PaymentHistories
            .FirstOrDefaultAsync(h => h.PaymentIntentId == paymentIntentId, cancellationToken);
    
        if (paymentHistory == null)
            return null;
    
        return await GetByIdAsync(paymentHistory.OrderId, cancellationToken);
    }
}