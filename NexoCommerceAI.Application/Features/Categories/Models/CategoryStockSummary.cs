namespace NexoCommerceAI.Application.Features.Categories.Models;

public class CategoryStockSummary
{
    public string? CategoryName { get; set; } = default!;
    public int TotalUnits { get; set; }
    public decimal TotalValue { get; set; }
    public int ProductCount { get; set; }
}