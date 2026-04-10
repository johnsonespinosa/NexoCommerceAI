using NexoCommerceAI.Application.Features.Categories.Models;

namespace NexoCommerceAI.Application.Features.Products.Models;

public class TotalStockValueResponse
{
    public int TotalUnits { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public int ActiveProductsCount { get; set; }
    public int LowStockProductsCount { get; set; }
    public int OutOfStockProductsCount { get; set; }
    public Dictionary<Guid, CategoryStockSummary>? StockByCategory { get; set; }
}