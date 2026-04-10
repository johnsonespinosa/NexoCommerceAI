using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Roles.Commands;

[InvalidateCache("roles_list", "role_by_id", "role_by_name")]
public class ActivateRoleCommand(Guid id) : IRequest<bool>
{
    public Guid Id { get; init; } = id;
}