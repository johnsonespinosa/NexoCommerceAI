using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Roles.Commands;

[InvalidateCache("roles_list", "role_by_id", "role_by_name")]
public class DeactivateRoleCommand(Guid id) : IRequest<bool>
{
    public Guid Id { get; init; } = id;
}