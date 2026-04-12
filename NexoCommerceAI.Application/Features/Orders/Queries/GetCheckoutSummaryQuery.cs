using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Queries;

[Cacheable("checkout_summary")]
public record GetCheckoutSummaryQuery(Guid UserId) : IRequest<CheckoutSummaryResponse>;