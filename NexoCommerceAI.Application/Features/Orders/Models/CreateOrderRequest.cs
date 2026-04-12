namespace NexoCommerceAI.Application.Features.Orders.Models;

public record CreateOrderRequest
{
    public AddressDto ShippingAddress { get; init; } = default!;
    public AddressDto BillingAddress { get; init; } = default!;
    public string? CustomerNotes { get; init; }
}