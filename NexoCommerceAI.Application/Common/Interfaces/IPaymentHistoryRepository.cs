using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IPaymentHistoryRepository : IRepositoryAsync<PaymentHistory>
{
    Task<IReadOnlyList<PaymentHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}