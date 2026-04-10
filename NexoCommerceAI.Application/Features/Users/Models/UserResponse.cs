namespace NexoCommerceAI.Application.Features.Users.Models;

public record UserResponse(
    Guid Id,
    string UserName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);