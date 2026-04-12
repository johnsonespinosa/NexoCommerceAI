using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Carts.Commands;

[InvalidateCache("cart", "cart_summary")]
public record ClearCartCommand(Guid UserId) : IRequest<bool>;