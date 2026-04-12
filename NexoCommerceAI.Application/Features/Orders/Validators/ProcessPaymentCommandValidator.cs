using FluentValidation;
using NexoCommerceAI.Application.Features.Orders.Commands;

namespace NexoCommerceAI.Application.Features.Orders.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
        
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("PaymentMethodId is required");
        
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .Matches(@"^\d{16}$").WithMessage("Invalid card number");
        
        RuleFor(x => x.ExpiryMonth)
            .NotEmpty().WithMessage("Expiry month is required")
            .Matches(@"^(0[1-9]|1[0-2])$").WithMessage("Invalid expiry month");
        
        RuleFor(x => x.ExpiryYear)
            .NotEmpty().WithMessage("Expiry year is required")
            .Matches(@"^\d{4}$").WithMessage("Invalid expiry year");
        
        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required")
            .Matches(@"^\d{3,4}$").WithMessage("Invalid CVV");
        
        RuleFor(x => x.CardHolderName)
            .NotEmpty().WithMessage("Card holder name is required");
    }
}