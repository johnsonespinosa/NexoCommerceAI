namespace NexoCommerceAI.Application.Features.Products.Models;

public class BulkStockUpdateError
{
    public Guid ProductId { get; set; }
    public string ErrorMessage { get; set; } = default!;
    public int AttemptedStock { get; set; }
}