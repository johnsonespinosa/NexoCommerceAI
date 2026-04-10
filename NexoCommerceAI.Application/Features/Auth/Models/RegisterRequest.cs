namespace NexoCommerceAI.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string ConfirmPassword
);