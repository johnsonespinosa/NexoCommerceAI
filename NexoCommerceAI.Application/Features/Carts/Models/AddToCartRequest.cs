namespace NexoCommerceAI.Application.Features.Carts.Models;

public record AddToCartRequest(Guid ProductId, int Quantity);