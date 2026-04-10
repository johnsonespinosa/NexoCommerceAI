// Application/Features/Products/Commands/UpdateProductStock/UpdateProductStockCommandValidator.cs

using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (productId, cancellation) =>
                await productRepository.ExistsAsync(productId, cancellation))
            .WithMessage("Product does not exist");
        
        RuleFor(x => x.NewStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .LessThanOrEqualTo(999999).WithMessage("Stock cannot exceed 999,999");
    }
}