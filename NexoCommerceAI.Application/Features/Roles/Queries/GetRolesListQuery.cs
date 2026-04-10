using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Roles.Models;

namespace NexoCommerceAI.Application.Features.Roles.Queries;

[Cacheable("roles_list")]
public class GetRolesListQuery : IRequest<PaginatedResult<RoleResponse>>
{
    public PaginationParams Pagination { get; init; } = new();
}