using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Application.Features.Carts.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing shopping cart operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class CartController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets the current user's shopping cart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The user's cart with all items.</returns>
    /// <response code="200">Returns the user's cart.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartResponse>> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetCartQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result ?? new CartResponse { UserId = userId, Items = new List<CartItemResponse>() });
    }

    /// <summary>
    /// Adds a product to the shopping cart.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="quantity">The quantity to add (default is 1).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated cart.</returns>
    /// <response code="200">Returns the updated cart.</response>
    /// <response code="400">If the quantity is invalid or product is not available.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product is not found.</response>
    [HttpPost("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> AddToCart(
        Guid productId,
        [FromQuery] int quantity = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new AddToCartCommand(userId, productId, quantity);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Updates the quantity of a product in the cart.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="quantity">The new quantity (set to 0 to remove).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated cart.</returns>
    /// <response code="200">Returns the updated cart.</response>
    /// <response code="400">If the quantity is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product or cart is not found.</response>
    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateCartItemQuantity(
        Guid productId,
        [FromBody] int quantity,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateCartItemQuantityCommand(userId, productId, quantity);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Removes a product from the shopping cart.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to remove.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The updated cart.</returns>
    /// <response code="200">Returns the updated cart.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the product or cart is not found.</response>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> RemoveFromCart(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new RemoveFromCartCommand(userId, productId);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Clears all items from the shopping cart.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the cart was successfully cleared.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ClearCart(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var command = new ClearCartCommand(userId);
        await _mediator.Send(command, cancellationToken);
        
        return NoContent();
    }

    /// <summary>
    /// Gets the current user's cart summary (total items and amount).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Cart summary information.</returns>
    /// <response code="200">Returns the cart summary.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CartSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartSummaryResponse>> GetCartSummary(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetCartQuery(userId);
        var cart = await _mediator.Send(query, cancellationToken);
        
        return Ok(new CartSummaryResponse
        {
            TotalItems = cart?.TotalItems ?? 0,
            TotalAmount = cart?.TotalAmount ?? 0,
            ItemCount = cart?.Items.Count ?? 0
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return userId;
    }
}