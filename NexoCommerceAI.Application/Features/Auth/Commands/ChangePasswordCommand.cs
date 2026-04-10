using MediatR;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class ChangePasswordCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
    public string ConfirmNewPassword { get; init; } = default!;
}