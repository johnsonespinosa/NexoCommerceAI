namespace NexoCommerceAI.Application.Features.Roles.Models;

public record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int UserCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);