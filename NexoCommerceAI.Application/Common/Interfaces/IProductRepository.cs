using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IProductRepository : IRepositoryAsync<Product>
{
    Task<Product?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> ExistBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<int> GetTotalStockValueAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<int> GetProductCountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    // Nuevos métodos
    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsOnSaleAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default);
    Task<bool> DecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm, int take = 20, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetStockForMultipleProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task<bool> ExistByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPaginatedWithFiltersAsync(
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
        CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountWithFiltersAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isFeatured = null,
        bool? isOnSale = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
}