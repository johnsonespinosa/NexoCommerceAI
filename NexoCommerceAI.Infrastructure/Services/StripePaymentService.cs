using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using NexoCommerceAI.Application.Features.Payments.Models;
using Stripe;

namespace NexoCommerceAI.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        // Configurar Stripe en modo simulación/mock
        if (_settings.UseMock)
        {
            StripeConfiguration.ApiKey = "sk_test_mock_key_for_development";
        }
        else
        {
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment of {Amount} {Currency}", request.Amount, request.Currency);

        try
        {
            if (_settings.UseMock)
            {
                return await ProcessMockPaymentAsync(request);
            }

            return await ProcessRealStripePaymentAsync(request);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment failed: {Error}", ex.Message);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment");
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred"
            };
        }
    }

    private async Task<PaymentResult> ProcessMockPaymentAsync(PaymentRequest request)
    {
        // Simular delay de procesamiento
        await Task.Delay(500);
    
        // Simular comportamiento de Stripe
        var isSuccessful = request.CardNumber switch
        {
            "4242424242424242" => true,  // Visa - éxito
            "4000000000000002" => false, // Visa - declinada
            "5555555555554444" => true,  // Mastercard - éxito
            "378282246310005" => true,   // Amex - éxito
            _ => true  // Por defecto éxito
        };

        if (!isSuccessful)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = "Payment declined by the bank"
            };
        }

        var paymentIntentId = $"pi_mock_{Guid.NewGuid():N}";
        var clientSecret = $"pi_mock_secret_{Guid.NewGuid():N}";

        return new PaymentResult
        {
            Success = true,
            TransactionId = paymentIntentId,
            ClientSecret = clientSecret,
            Last4 = request.CardNumber?.Length >= 4 ? request.CardNumber[^4..] : "4242",
            CardType = GetCardType(request.CardNumber)
        };
    }

    private async Task<PaymentResult> ProcessRealStripePaymentAsync(PaymentRequest request)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Stripe usa centavos
            Currency = request.Currency.ToLower(),
            PaymentMethod = request.PaymentMethodId,
            ConfirmationMethod = "manual",
            Confirm = true,
            Metadata = new Dictionary<string, string>
            {
                { "OrderId", request.OrderId.ToString() }, // Corregido: Guid no nullable
                { "CustomerEmail", request.CustomerEmail ?? "unknown" }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        if (paymentIntent.Status == "succeeded")
        {
            return new PaymentResult
            {
                Success = true,
                TransactionId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Last4 = paymentIntent.PaymentMethod?.Card?.Last4,
                CardType = paymentIntent.PaymentMethod?.Card?.Brand
            };
        }

        return new PaymentResult
        {
            Success = false,
            ErrorMessage = $"Payment {paymentIntent.Status}"
        };
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

    public async Task<PaymentResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refunding payment {TransactionId} of {Amount}", transactionId, amount);

        if (_settings.UseMock)
        {
            await Task.Delay(300);
            return new PaymentResult
            {
                Success = true,
                TransactionId = $"ref_{transactionId}"
            };
        }

        try
        {
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = (long)(amount * 100)
            };
            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(refundOptions);

            return new PaymentResult
            {
                Success = true,
                TransactionId = refund.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund failed for {TransactionId}", transactionId);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}