using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Carts.Commands;

[InvalidateCache("cart", "cart_summary")]
public record AddToCartCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    decimal? SelectedPrice = null) : IRequest<CartResponse>;