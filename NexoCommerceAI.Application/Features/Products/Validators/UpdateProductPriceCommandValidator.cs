// Application/Features/Products/Commands/UpdateProductPrice/UpdateProductPriceCommandValidator.cs

using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (productId, cancellation) =>
                await productRepository.ExistsAsync(productId, cancellation))
            .WithMessage("Product does not exist");
        
        RuleFor(x => x.NewPrice)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).WithMessage("Price cannot exceed 1,000,000")
            .Must(price => Math.Round(price, 2) == price)
            .WithMessage("Price must have at most 2 decimal places");
        
        RuleFor(x => x.NewCompareAtPrice)
            .GreaterThan(0).When(x => x.NewCompareAtPrice.HasValue)
            .WithMessage("Compare at price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).When(x => x.NewCompareAtPrice.HasValue)
            .WithMessage("Compare at price cannot exceed 1,000,000")
            .Must((command, compareAtPrice) => 
                !compareAtPrice.HasValue || compareAtPrice.Value > command.NewPrice)
            .WithMessage("Compare at price must be greater than regular price")
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || compareAtPrice.Value <= command.NewPrice * 2)
            .WithMessage("Compare at price cannot be more than double the regular price")
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || compareAtPrice.Value >= command.NewPrice * 1.1m)
            .WithMessage("Compare at price should be at least 10% higher than regular price for a meaningful discount");
    }
}