using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Carts.Commands;

[InvalidateCache("cart", "cart_summary")]
public record RemoveFromCartCommand(
    Guid UserId,
    Guid ProductId) : IRequest<CartResponse>;