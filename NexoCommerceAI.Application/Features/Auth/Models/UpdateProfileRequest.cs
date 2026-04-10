namespace NexoCommerceAI.Application.Features.Auth.Models;

public record UpdateProfileRequest(
    string UserName,
    string Email
);