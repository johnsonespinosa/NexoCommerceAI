using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
{
    public ActivateProductCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (productId, cancellation) =>
                await productRepository.ExistsAsync(productId, cancellation))
            .WithMessage("Product does not exist");
    }
}