using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.UnitTests.Products.TestHelpers;

internal sealed class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _items = [];

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _items.Add(product);
        return Task.CompletedTask;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.SingleOrDefault(p => p.Id == id));
    }

    public Task<bool> ExistsBySkuAsync(string sku, Guid? excludingProductId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.Any(p => p.Sku == sku && (!excludingProductId.HasValue || p.Id != excludingProductId.Value)));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<PaginatedResult<Product>> GetAllAsync(int page, int pageSize, string? search = null, Guid? categoryId = null, string? sku = null, bool? isFeatured = null, CancellationToken cancellationToken = default)
    {
        var query = _items.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase));
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(sku)) query = query.Where(p => p.Sku == sku);
        if (isFeatured.HasValue) query = query.Where(p => p.IsFeatured == isFeatured.Value);

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedResult<Product>(items, page, pageSize, total);
        return Task.FromResult(result);
    }

    // For tests
    public IReadOnlyList<Product> Items => _items;
}

internal sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly HashSet<Guid> _ids = [];

    public InMemoryCategoryRepository(params Guid[] ids)
    {
        foreach (var id in ids) _ids.Add(id);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_ids.Contains(id));
    }
}
