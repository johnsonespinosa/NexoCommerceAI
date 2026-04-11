using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Models;

public record CategoryResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    public CategoryResponse()
    {
    }
    
    public CategoryResponse(Category category, int productCount = 0)
    {
        Id = category.Id;
        Name = category.Name;
        Slug = category.Slug;
        IsActive = category.IsActive;
        ProductCount = productCount;
        CreatedAt = category.CreatedAt;
        UpdatedAt = category.UpdatedAt;
    }
}