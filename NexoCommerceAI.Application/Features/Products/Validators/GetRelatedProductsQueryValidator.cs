using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetRelatedProductsQueryValidator : AbstractValidator<GetRelatedProductsQuery>
{
    public GetRelatedProductsQueryValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (productId, cancellation) =>
                await productRepository.ExistsAsync(productId, cancellation))
            .WithMessage("Product does not exist");
        
        RuleFor(x => x.Take)
            .GreaterThan(0).WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("Take cannot exceed 20");
    }
}