namespace NexoCommerceAI.Application.Features.Products.Models;

public class ProductDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}