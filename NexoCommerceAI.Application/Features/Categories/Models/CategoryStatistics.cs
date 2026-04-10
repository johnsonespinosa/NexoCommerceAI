namespace NexoCommerceAI.Application.Features.Categories.Models;

public class CategoryStatistics
{
    public string CategoryName { get; set; } = default!;
    public int ProductCount { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalStockUnits { get; set; }
    public decimal TotalStockValue { get; set; }
    public decimal AveragePrice { get; set; }
    public int ProductsOnSale { get; set; }
}