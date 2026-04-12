namespace NexoCommerceAI.Application.Features.Orders.Models;

public record AddressDto
{
    public string Street { get; init; } = default!;
    public string City { get; init; } = default!;
    public string? State { get; init; }
    public string ZipCode { get; init; } = default!;
    public string Country { get; init; } = default!;
}