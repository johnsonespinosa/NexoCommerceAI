using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Id == id && !product.IsDeleted, cancellationToken);
    }

    public async Task<PaginatedResult<Product>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        Guid? categoryId = null,
        string? sku = null,
        bool? isFeatured = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Products
            .Include(product => product.Images)
            .AsNoTracking()
            .Where(product => !product.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim().ToLowerInvariant();
            query = query.Where(product =>
                product.Name.ToLower().Contains(searchValue) ||
                (product.Description ?? string.Empty).ToLower().Contains(searchValue) ||
                product.Sku.ToLower().Contains(searchValue));
        }

        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var normalizedSku = sku.Trim();
            query = query.Where(product => product.Sku == normalizedSku);
        }

        if (isFeatured.HasValue)
        {
            query = query.Where(product => product.IsFeatured == isFeatured.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(product => product.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Product>(items, page, pageSize, totalCount);
    }

    public Task<bool> ExistsBySkuAsync(string sku, Guid? excludingProductId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.Where(product => product.Sku == sku && !product.IsDeleted);

        if (excludingProductId.HasValue)
        {
            query = query.Where(product => product.Id != excludingProductId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
