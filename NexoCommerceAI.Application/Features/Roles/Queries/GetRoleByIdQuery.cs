using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Roles.Models;

namespace NexoCommerceAI.Application.Features.Roles.Queries;

[Cacheable("role_by_id")]
public class GetRoleByIdQuery(Guid id) : IRequest<RoleResponse?>
{
    public Guid Id { get; init; } = id;
}