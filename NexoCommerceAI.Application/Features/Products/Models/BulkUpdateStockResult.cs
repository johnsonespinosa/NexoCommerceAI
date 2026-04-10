namespace NexoCommerceAI.Application.Features.Products.Models;

public class BulkUpdateStockResult
{
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public IReadOnlyList<BulkStockUpdateError> Errors { get; set; } = new List<BulkStockUpdateError>();
    
    public bool AllSucceeded => FailureCount == 0;
    public bool PartialSuccess => SuccessCount > 0 && FailureCount > 0;
    public bool AllFailed => SuccessCount == 0 && FailureCount > 0;
}