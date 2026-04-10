namespace NexoCommerceAI.Application.Features.Products.Models;

public class UpdateProductRequest
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string? Sku { get; init; }
    public int? Stock { get; init; }
    public bool? IsFeatured { get; init; }
    public Guid? CategoryId { get; init; }
}