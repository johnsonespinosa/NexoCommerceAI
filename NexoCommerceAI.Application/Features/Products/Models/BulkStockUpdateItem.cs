namespace NexoCommerceAI.Application.Features.Products.Models;

public class BulkStockUpdateItem
{
    public Guid ProductId { get; init; }
    public int NewStock { get; init; }
}