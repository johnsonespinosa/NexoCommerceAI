using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Roles.Models;

namespace NexoCommerceAI.Application.Features.Roles.Queries;

[Cacheable("role_by_name")]
public class GetRoleByNameQuery(string name) : IRequest<RoleResponse?>
{
    public string Name { get; init; } = name;
}