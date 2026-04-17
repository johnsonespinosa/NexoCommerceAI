using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Abstractions;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Product>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? categoryId = null,
        string? sku = null,
        bool? isFeatured = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludingProductId = null, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
