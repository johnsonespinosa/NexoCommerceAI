using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Application.Features.Orders.Queries;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class GetCheckoutSummaryQueryHandler(
    ICartRepository cartRepository,
    ILogger<GetCheckoutSummaryQueryHandler> logger)
    : IRequestHandler<GetCheckoutSummaryQuery, CheckoutSummaryResponse>
{
    public async Task<CheckoutSummaryResponse> Handle(GetCheckoutSummaryQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting checkout summary for user {UserId}", request.UserId);
        
        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        
        decimal subtotal = 0;
        var totalItems = 0;
        
        if (cart != null && cart.Items.Count != 0)
        {
            subtotal = cart.TotalAmount;
            totalItems = cart.TotalItems;
        }
        
        var taxAmount = subtotal * 0.21m; // 21% IVA
        var shippingAmount = CalculateShipping(subtotal);
        var totalAmount = subtotal + taxAmount + shippingAmount;
        
        return new CheckoutSummaryResponse
        {
            CartSummary = new CartSummary
            {
                TotalItems = totalItems,
                TotalAmount = subtotal
            },
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            DiscountAmount = 0,
            TotalAmount = totalAmount,
            ShippingMethods = GetShippingMethods(),
            PaymentMethods = GetPaymentMethods()
        };
    }
    
    private static decimal CalculateShipping(decimal subtotal)
    {
        // Envío gratis para pedidos > 50€
        if (subtotal > 50)
            return 0;
        
        return 4.99m;
    }
    
    private static IReadOnlyList<ShippingMethodDto> GetShippingMethods()
    {
        return new List<ShippingMethodDto>
        {
            new() { Id = "standard", Name = "Standard Shipping", Price = 4.99m, EstimatedDays = 3 },
            new() { Id = "express", Name = "Express Shipping", Price = 9.99m, EstimatedDays = 1 }
        };
    }
    
    private static IReadOnlyList<PaymentMethodDto> GetPaymentMethods()
    {
        return new List<PaymentMethodDto>
        {
            new() { Id = "card", Name = "Credit/Debit Card", Icon = "💳" },
            new() { Id = "paypal", Name = "PayPal", Icon = "🅿️" }
        };
    }
}