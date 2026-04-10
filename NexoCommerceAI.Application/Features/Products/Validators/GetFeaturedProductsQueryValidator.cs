using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetFeaturedProductsQueryValidator : AbstractValidator<GetFeaturedProductsQuery>
{
    public GetFeaturedProductsQueryValidator()
    {
        RuleFor(x => x.Take)
            .GreaterThan(0).WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Take cannot exceed 100");
    }
}