using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Wishlists.Models;

namespace NexoCommerceAI.Application.Features.Wishlists.Commands;

[InvalidateCache("wishlist")]
public record AddToWishlistCommand(
    Guid UserId,
    Guid ProductId,
    string? WishlistName = null) : IRequest<WishlistResponse>;