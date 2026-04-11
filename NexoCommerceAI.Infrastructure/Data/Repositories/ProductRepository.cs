using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Specifications;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class ProductRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Product>(dbContext), IProductRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new ProductByIdWithCategorySpec(id);
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }
    
    // Sobrescribir GetByIdAsync para incluir imágenes
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))  // Solo imágenes activas
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }
    
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted, cancellationToken);
    }
    
    public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;
            
        var spec = new ProductBySlugSpec(slug);
        return !await AnyAsync(spec, cancellationToken);
    }

    public async Task<bool> ExistBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;
            
        return await _dbContext.Products
            .AnyAsync(p => p.Sku == sku && !p.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;
            
        return await _dbContext.Products
            .AnyAsync(p => p.Slug == slug && !p.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Where(p => p.Stock <= threshold && p.Stock > 0 && !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Stock)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetTotalStockValueAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => !p.IsDeleted && p.IsActive)
            .SumAsync(p => p.Stock, cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var spec = new ProductByIdSpec(id);
        return await AnyAsync(spec, cancellationToken);
    }
    
    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Where(p => p.IsActive && !p.IsDeleted && p.Stock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Where(p => !p.IsDeleted && p.IsActive)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTermLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTermLower)) ||
                p.Sku.ToLower().Contains(searchTermLower));
        }
        
        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetProductCountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.IsFeatured && p.IsActive && !p.IsDeleted && p.Stock > 0)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsOnSaleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.CompareAtPrice > p.Price && p.IsActive && !p.IsDeleted && p.Stock > 0)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);
            
        if (product == null)
            return false;
            
        product.UpdateStock(newStock);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted && p.IsActive, cancellationToken);
            
        if (product == null)
            return false;
            
        try
        {
            product.DecreaseStock(quantity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm, int take = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Product>();
            
        return await _dbContext.Products
            .Where(p => (p.Name.Contains(searchTerm) || 
                        p.Description!.Contains(searchTerm) || 
                        p.Sku.Contains(searchTerm)) &&
                        p.IsActive && 
                        !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetStockForMultipleProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, int>();
            
        return await _dbContext.Products
            .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, p => p.Stock, cancellationToken);
    }

    public async Task<bool> ExistByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AnyAsync(p => p.Name == name && !p.IsDeleted, cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPaginatedWithFiltersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isFeatured = null,
        bool? isOnSale = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => i.IsActive))
            .Where(p => !p.IsDeleted && p.IsActive)
            .AsQueryable();
        
        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTermLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTermLower)) ||
                p.Sku.ToLower().Contains(searchTermLower));
        }
        
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        
        if (isFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == isFeatured.Value);
        
        if (isOnSale.HasValue && isOnSale.Value)
            query = query.Where(p => p.CompareAtPrice != null && p.CompareAtPrice > p.Price);
        
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
        
        // Obtener total
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Aplicar ordenamiento
        query = ApplySorting(query, sortBy, sortDescending);
        
        // Aplicar paginación
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }

    public async Task<int> GetTotalCountWithFiltersAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isFeatured = null,
        bool? isOnSale = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .Where(p => !p.IsDeleted && p.IsActive)
            .AsQueryable();
        
        // Aplicar filtros
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTermLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTermLower)) ||
                p.Sku.ToLower().Contains(searchTermLower));
        }
        
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);
        
        if (isFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == isFeatured.Value);
        
        if (isOnSale.HasValue && isOnSale.Value)
            query = query.Where(p => p.CompareAtPrice != null && p.CompareAtPrice > p.Price);
        
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
        
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductImage>> GetProductImagesAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductImages
            .Where(i => i.ProductId == productId && i.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .ThenByDescending(i => i.IsMain)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(p => p.CreatedAt);
        
        var sortByLower = sortBy.ToLower();
        
        return sortByLower switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "stock" => sortDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}