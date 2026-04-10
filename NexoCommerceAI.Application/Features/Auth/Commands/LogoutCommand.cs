using MediatR;

namespace NexoCommerceAI.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}