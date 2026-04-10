namespace NexoCommerceAI.Application.Features.Auth.Models;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);