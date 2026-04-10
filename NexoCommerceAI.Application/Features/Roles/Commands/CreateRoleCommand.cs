using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Roles.Models;

namespace NexoCommerceAI.Application.Features.Roles.Commands;

[InvalidateCache("roles_list", "role_by_id", "role_by_name")]
public class CreateRoleCommand : IRequest<RoleResponse>
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
}