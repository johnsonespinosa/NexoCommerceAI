using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetProductsOnSaleQueryValidator : AbstractValidator<GetProductsOnSaleQuery>
{
    public GetProductsOnSaleQueryValidator()
    {
        RuleFor(x => x.Take)
            .GreaterThan(0).When(x => x.Take.HasValue)
            .WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(100).When(x => x.Take.HasValue)
            .WithMessage("Take cannot exceed 100");
    }
}