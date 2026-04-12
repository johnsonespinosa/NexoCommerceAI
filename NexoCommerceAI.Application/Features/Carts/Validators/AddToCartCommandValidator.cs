using FluentValidation;
using NexoCommerceAI.Application.Features.Carts.Commands;

namespace NexoCommerceAI.Application.Features.Carts.Validators;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
        
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(999).WithMessage("Quantity cannot exceed 999");
        
        RuleFor(x => x.SelectedPrice)
            .GreaterThan(0).When(x => x.SelectedPrice.HasValue)
            .WithMessage("Selected price must be greater than 0");
    }
}