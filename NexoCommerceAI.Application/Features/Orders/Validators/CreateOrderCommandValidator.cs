using FluentValidation;
using NexoCommerceAI.Application.Features.Orders.Commands;

namespace NexoCommerceAI.Application.Features.Orders.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
        
        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required");
        
        RuleFor(x => x.BillingAddress)
            .NotNull().WithMessage("Billing address is required");
        
        When(x => x.ShippingAddress != null, () =>
        {
            RuleFor(x => x.ShippingAddress.Street)
                .NotEmpty().WithMessage("Shipping street is required");
            RuleFor(x => x.ShippingAddress.City)
                .NotEmpty().WithMessage("Shipping city is required");
            RuleFor(x => x.ShippingAddress.ZipCode)
                .NotEmpty().WithMessage("Shipping zip code is required");
            RuleFor(x => x.ShippingAddress.Country)
                .NotEmpty().WithMessage("Shipping country is required");
        });
        
        When(x => x.BillingAddress != null, () =>
        {
            RuleFor(x => x.BillingAddress.Street)
                .NotEmpty().WithMessage("Billing street is required");
            RuleFor(x => x.BillingAddress.City)
                .NotEmpty().WithMessage("Billing city is required");
            RuleFor(x => x.BillingAddress.ZipCode)
                .NotEmpty().WithMessage("Billing zip code is required");
            RuleFor(x => x.BillingAddress.Country)
                .NotEmpty().WithMessage("Billing country is required");
        });
    }
}