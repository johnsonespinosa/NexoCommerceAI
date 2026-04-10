namespace NexoCommerceAI.Application.Features.Categories.Models;

public record CategoryDto(Guid Id, string Name, string Slug, bool IsActive, DateTime CreatedAt);