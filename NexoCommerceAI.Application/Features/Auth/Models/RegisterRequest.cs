namespace NexoCommerceAI.Application.Features.Auth.Models;

public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword
);