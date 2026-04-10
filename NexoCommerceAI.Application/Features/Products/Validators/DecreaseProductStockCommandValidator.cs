using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class DecreaseProductStockCommandValidator : AbstractValidator<DecreaseProductStockCommand>
{
    public DecreaseProductStockCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (productId, cancellation) =>
                await productRepository.ExistsAsync(productId, cancellation))
            .WithMessage("Product does not exist");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(999999).WithMessage("Quantity cannot exceed 999,999");
    }
}