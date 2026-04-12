using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Features.Payments.Commands;
using NexoCommerceAI.Application.Features.Payments.Models;

namespace NexoCommerceAI.Api.Controllers.v1;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Crea un PaymentIntent para una orden
    /// </summary>
    [HttpPost("create-payment-intent/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        Guid orderId,
        [FromBody] string? paymentMethodId = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePaymentIntentCommand(orderId, paymentMethodId);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirma el pago de una orden
    /// </summary>
    [HttpPost("confirm-payment/{orderId:guid}")]
    [ProducesResponseType(typeof(ConfirmPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfirmPaymentResponse>> ConfirmPayment(
        Guid orderId,
        [FromBody] string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var command = new ConfirmPaymentCommand(orderId, paymentIntentId);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}