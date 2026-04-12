using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Application.Features.Wishlists.Commands;
using NexoCommerceAI.Application.Features.Wishlists.Models;
using NexoCommerceAI.Application.Features.Wishlists.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing user wishlists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class WishlistController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the current user's wishlist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The user's wishlist with all items.</returns>
    /// <response code="200">Returns the user's wishlist.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WishlistResponse>> GetWishlist(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetWishlistQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result ?? new WishlistResponse { Name = "Default Wishlist", Items = new List<WishlistItemResponse>() });
    }

    /// <summary>
    /// Adds a product to the wishlist.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated wishlist.</returns>
    /// <response code="200">Returns the updated wishlist.</response>
    /// <response code="400">If the product is already in the wishlist.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPost("items/{productId:guid}")]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistResponse>> AddToWishlist(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new AddToWishlistCommand(userId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Removes a product from the wishlist.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated wishlist.</returns>
    /// <response code="200">Returns the updated wishlist.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product or wishlist is not found.</response>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistResponse>> RemoveFromWishlist(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new RemoveFromWishlistCommand(userId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Checks if a product is in the user's wishlist.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>True if the product is in the wishlist, false otherwise.</returns>
    /// <response code="200">Returns the wishlist status.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("items/{productId:guid}/exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<bool>> IsInWishlist(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var wishlist = await _mediator.Send(new GetWishlistQuery(userId), cancellationToken);
        
        var exists = wishlist?.Items.Any(i => i.ProductId == productId) ?? false;
        
        return Ok(exists);
    }

    /// <summary>
    /// Moves a product from wishlist to cart.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="quantity">The quantity to add to cart (default is 1).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated cart.</returns>
    /// <response code="200">Returns the updated cart.</response>
    /// <response code="400">If the quantity is invalid or product is not available.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product or wishlist is not found.</response>
    [HttpPost("items/{productId:guid}/move-to-cart")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> MoveToCart(
        Guid productId,
        [FromQuery] int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        
        // Primero agregar al carrito
        var addToCartCommand = new AddToCartCommand(userId, productId, quantity);
        var cart = await _mediator.Send(addToCartCommand, cancellationToken);
        
        // Luego eliminar de la wishlist
        var removeFromWishlistCommand = new RemoveFromWishlistCommand(userId, productId);
        await _mediator.Send(removeFromWishlistCommand, cancellationToken);
        
        return Ok(cart);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return userId;
    }
}