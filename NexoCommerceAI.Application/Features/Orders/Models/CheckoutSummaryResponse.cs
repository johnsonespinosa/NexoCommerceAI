using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Orders.Models;

public record CheckoutSummaryResponse
{
    public CartSummary CartSummary { get; init; } = default!;
    public decimal Subtotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<ShippingMethodDto> ShippingMethods { get; init; } = new List<ShippingMethodDto>();
    public IReadOnlyList<PaymentMethodDto> PaymentMethods { get; init; } = new List<PaymentMethodDto>();
}