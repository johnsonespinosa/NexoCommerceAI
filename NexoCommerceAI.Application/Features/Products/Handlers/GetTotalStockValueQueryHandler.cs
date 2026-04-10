using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetTotalStockValueQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<GetTotalStockValueQueryHandler> logger)
    : IRequestHandler<GetTotalStockValueQuery, TotalStockValueResponse>
{
    public async Task<TotalStockValueResponse> Handle(GetTotalStockValueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Calculating total stock value");
            
            // Obtener todos los productos activos y no eliminados
            var products = await productRepository.GetActiveProductsAsync(cancellationToken);
            
            var activeProducts = products.Where(p => p is { IsActive: true, IsDeleted: false }).ToList();
            
            // Calcular totales
            var totalUnits = activeProducts.Sum(p => p.Stock);
            var totalValue = activeProducts.Sum(p => p.Price * p.Stock);
            var averagePrice = activeProducts.Count != 0 ? activeProducts.Average(p => p.Price) : 0;
            
            // Contar productos con stock bajo y sin stock
            var lowStockProductsCount = activeProducts.Count(p => p.IsLowStock());
            var outOfStockProductsCount = activeProducts.Count(p => p.Stock == 0);
            
            logger.LogDebug("Total stock value calculated - Units: {TotalUnits}, Value: {TotalValue:C}, Avg Price: {AveragePrice:C}", 
                totalUnits, totalValue, averagePrice);
            
            // Obtener resumen por categoría (opcional)
            var categories = await categoryRepository.ListAsync(cancellationToken);
            var stockByCategory = new Dictionary<Guid, CategoryStockSummary>();
            
            foreach (var category in categories.Where(c => c is { IsActive: true, IsDeleted: false }))
            {
                var categoryProducts = activeProducts.Where(p => p.CategoryId == category.Id).ToList();
                if (categoryProducts.Count != 0)
                {
                    stockByCategory[category.Id] = new CategoryStockSummary
                    {
                        CategoryName = category.Name,
                        TotalUnits = categoryProducts.Sum(p => p.Stock),
                        TotalValue = categoryProducts.Sum(p => p.Price * p.Stock),
                        ProductCount = categoryProducts.Count
                    };
                }
            }
            
            var response = new TotalStockValueResponse
            {
                TotalUnits = totalUnits,
                TotalValue = totalValue,
                AveragePrice = averagePrice,
                ActiveProductsCount = activeProducts.Count,
                LowStockProductsCount = lowStockProductsCount,
                OutOfStockProductsCount = outOfStockProductsCount,
                StockByCategory = stockByCategory
            };
            
            logger.LogInformation("Total stock value calculation completed - Active Products: {Count}, Total Value: {TotalValue:C}", 
                response.ActiveProductsCount, response.TotalValue);
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating total stock value");
            throw;
        }
    }
}