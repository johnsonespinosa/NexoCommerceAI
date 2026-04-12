using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Carts.Queries;

[Cacheable("cart")]
public record GetCartQuery(Guid UserId) : IRequest<CartResponse?>;