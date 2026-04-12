using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Orders.Models;

public record OrderResponse
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public Guid UserId { get; init; }
    public string Status { get; init; } = default!;
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = new List<OrderItemResponse>();
    public decimal Subtotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public AddressDto ShippingAddress { get; init; } = default!;
    public AddressDto BillingAddress { get; init; } = default!;
    public string? TrackingNumber { get; init; }
    public string? CarrierName { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CustomerNotes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    public static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSku = i.ProductSku,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice,
                ProductImageUrl = i.ProductImageUrl
            }).ToList(),
            Subtotal = order.Subtotal.Amount,
            TaxAmount = order.TaxAmount.Amount,
            ShippingAmount = order.ShippingAmount.Amount,
            DiscountAmount = order.DiscountAmount.Amount,
            TotalAmount = order.TotalAmount.Amount,
            ShippingAddress = new AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode,
                Country = order.ShippingAddress.Country
            },
            BillingAddress = new AddressDto
            {
                Street = order.BillingAddress.Street,
                City = order.BillingAddress.City,
                State = order.BillingAddress.State,
                ZipCode = order.BillingAddress.ZipCode,
                Country = order.BillingAddress.Country
            },
            TrackingNumber = order.TrackingNumber,
            CarrierName = order.CarrierName,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt,
            CustomerNotes = order.CustomerNotes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}



