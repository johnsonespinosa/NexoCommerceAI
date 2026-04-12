using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Wishlists.Models;

namespace NexoCommerceAI.Application.Features.Wishlists.Queries;

[Cacheable("wishlist")]
public record GetWishlistQuery(Guid UserId) : IRequest<WishlistResponse?>;