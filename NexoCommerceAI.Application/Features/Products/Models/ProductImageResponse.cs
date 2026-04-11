namespace NexoCommerceAI.Application.Features.Products.Models;

public class ProductImageResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public string? PublicId { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}