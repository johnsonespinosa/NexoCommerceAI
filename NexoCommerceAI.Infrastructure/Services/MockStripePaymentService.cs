// Infrastructure/Services/MockStripePaymentService.cs
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Infrastructure.Services;

public class MockStripePaymentService(ILogger<MockStripePaymentService> logger) : IPaymentService
{
    private readonly Random _random = new();

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing payment of {Amount} {Currency}", request.Amount, request.Currency);
        
        // Simular procesamiento
        var success = _random.NextDouble() > 0.1; // 90% de éxito
        
        if (success)
        {
            var transactionId = $"stripe_{Guid.NewGuid():N}";
            
            logger.LogInformation("Payment successful: {TransactionId}", transactionId);
            
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Last4 = request.CardNumber?.Length >= 4 ? request.CardNumber[^4..] : "4242",
                CardType = GetCardType(request.CardNumber)
            });
        }
        
        logger.LogWarning("Payment failed");
        
        return Task.FromResult(new PaymentResult
        {
            Success = false,
            ErrorMessage = "Payment declined by the bank"
        });
    }
    
    public Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Refunding payment {TransactionId} of {Amount}", transactionId, amount);
        
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"refund_{transactionId}"
        });
    }
    
    private static string GetCardType(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber)) return "Unknown";
        
        return cardNumber[0] switch
        {
            '4' => "Visa",
            '5' => "Mastercard",
            '3' => "American Express",
            '6' => "Discover",
            _ => "Unknown"
        };
    }
}