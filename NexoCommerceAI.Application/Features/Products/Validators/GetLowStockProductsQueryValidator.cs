using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetLowStockProductsQueryValidator : AbstractValidator<GetLowStockProductsQuery>
{
    public GetLowStockProductsQueryValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThan(0).WithMessage("Threshold must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Threshold cannot exceed 100");
    }
}