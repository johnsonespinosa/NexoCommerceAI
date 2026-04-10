using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductsStatisticsQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<GetProductsStatisticsQueryHandler> logger)
    : IRequestHandler<GetProductsStatisticsQuery, ProductsStatisticsResponse>
{
    public async Task<ProductsStatisticsResponse> Handle(GetProductsStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting products statistics");
            
            // Obtener todos los productos (incluyendo inactivos y eliminados para estadísticas)
            var allProducts = await productRepository.ListAsync(cancellationToken);
            
            // Calcular fechas para últimas actividades
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            
            // Totales generales
            var totalProducts = allProducts.Count;
            var activeProducts = allProducts.Count(p => p.IsActive && !p.IsDeleted);
            var inactiveProducts = allProducts.Count(p => !p.IsActive && !p.IsDeleted);
            var deletedProducts = allProducts.Count(p => p.IsDeleted);
            
            // Stock
            var activeNonDeletedProducts = allProducts.Where(p => p.IsActive && !p.IsDeleted).ToList();
            var totalStockUnits = activeNonDeletedProducts.Sum(p => p.Stock);
            var totalStockValue = activeNonDeletedProducts.Sum(p => p.Price * p.Stock);
            var lowStockProducts = activeNonDeletedProducts.Count(p => p.IsLowStock());
            var outOfStockProducts = activeNonDeletedProducts.Count(p => p.Stock == 0);
            
            // Precios
            var minPrice = activeNonDeletedProducts.Count != 0 ? activeNonDeletedProducts.Min(p => p.Price) : 0;
            var maxPrice = activeNonDeletedProducts.Count != 0 ? activeNonDeletedProducts.Max(p => p.Price) : 0;
            var averagePrice = activeNonDeletedProducts.Count != 0 ? activeNonDeletedProducts.Average(p => p.Price) : 0;
            
            // Destacados y ofertas
            var featuredProducts = activeNonDeletedProducts.Count(p => p.IsFeatured);
            var productsOnSale = activeNonDeletedProducts.Count(p => p.IsOnSale());
            
            // Últimas actividades
            var productsCreatedLastWeek = allProducts.Count(p => p.CreatedAt >= oneWeekAgo);
            var productsUpdatedLastWeek = allProducts.Count(p => p.UpdatedAt.HasValue && p.UpdatedAt.Value >= oneWeekAgo);
            var productsDeletedLastWeek = allProducts.Count(p => p.IsDeleted && p.UpdatedAt.HasValue && p.UpdatedAt.Value >= oneWeekAgo);
            
            logger.LogDebug("Statistics calculated - Total: {Total}, Active: {Active}, Stock Units: {StockUnits}, Value: {StockValue:C}", 
                totalProducts, activeProducts, totalStockUnits, totalStockValue);
            
            // Estadísticas por categoría
            var categories = await categoryRepository.ListAsync(cancellationToken);
            var statisticsByCategory = new Dictionary<string, CategoryStatistics>();
            
            foreach (var category in categories.Where(c => c is { IsActive: true, IsDeleted: false }))
            {
                var categoryProducts = activeNonDeletedProducts.Where(p => p.CategoryId == category.Id).ToList();
                
                statisticsByCategory[category.Name] = new CategoryStatistics
                {
                    CategoryName = category.Name,
                    ProductCount = categoryProducts.Count,
                    ActiveProducts = categoryProducts.Count(p => p.IsActive),
                    TotalStockUnits = categoryProducts.Sum(p => p.Stock),
                    TotalStockValue = categoryProducts.Sum(p => p.Price * p.Stock),
                    AveragePrice = categoryProducts.Any() ? categoryProducts.Average(p => p.Price) : 0,
                    ProductsOnSale = categoryProducts.Count(p => p.IsOnSale())
                };
            }
            
            var response = new ProductsStatisticsResponse
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                DeletedProducts = deletedProducts,
                TotalStockUnits = totalStockUnits,
                TotalStockValue = totalStockValue,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                AveragePrice = averagePrice,
                FeaturedProducts = featuredProducts,
                ProductsOnSale = productsOnSale,
                StatisticsByCategory = statisticsByCategory,
                ProductsCreatedLastWeek = productsCreatedLastWeek,
                ProductsUpdatedLastWeek = productsUpdatedLastWeek,
                ProductsDeletedLastWeek = productsDeletedLastWeek
            };
            
            logger.LogInformation("Products statistics completed - Active: {Active}/{Total}, Stock Value: {StockValue:C}, On Sale: {OnSale}", 
                response.ActiveProducts, response.TotalProducts, response.TotalStockValue, response.ProductsOnSale);
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting products statistics");
            throw;
        }
    }
}