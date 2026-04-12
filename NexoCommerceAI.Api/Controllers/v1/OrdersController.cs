using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Application.Features.Orders.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing customer orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    /// <summary>
    /// Gets a paginated list of orders for the current user.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10, max: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of orders.</returns>
    /// <response code="200">Returns the list of orders.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResult<OrderResponse>>> GetMyOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetOrdersByUserQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets an order by its ID.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">Returns the order.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the order is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetOrderByIdQuery(id, userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Gets an order by its order number.
    /// </summary>
    /// <param name="orderNumber">The order number (format: ORD-YYYYMMDD-XXXXXXXX).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">Returns the order.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the order is not found.</response>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrderByNumber(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetOrderByNumberQuery(orderNumber, userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Gets checkout summary with cart totals and available shipping/payment methods.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checkout summary information.</returns>
    /// <response code="200">Returns the checkout summary.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("checkout/summary")]
    [ProducesResponseType(typeof(CheckoutSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutSummaryResponse>> GetCheckoutSummary(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetCheckoutSummaryQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order from the user's cart.
    /// </summary>
    /// <param name="request">The create order request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">Returns the created order.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        
        var command = new CreateOrderCommand(
            userId,
            request.ShippingAddress,
            request.BillingAddress,
            request.CustomerNotes);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User not authenticated");
        
        return userId;
    }
}