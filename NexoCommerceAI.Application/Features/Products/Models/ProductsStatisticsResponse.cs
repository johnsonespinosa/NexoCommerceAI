using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Products.Models;

public class ProductsStatisticsResponse
{
    // Totales
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }
    public int DeletedProducts { get; set; }
    
    // Stock
    public int TotalStockUnits { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    
    // Precios
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
    
    // Destacados y ofertas
    public int FeaturedProducts { get; set; }
    public int ProductsOnSale { get; set; }
    
    // Por categoría
    public Dictionary<string, CategoryStatistics>? StatisticsByCategory { get; set; }
    
    // Últimas actividades
    public int ProductsCreatedLastWeek { get; set; }
    public int ProductsUpdatedLastWeek { get; set; }
    public int ProductsDeletedLastWeek { get; set; }
}

