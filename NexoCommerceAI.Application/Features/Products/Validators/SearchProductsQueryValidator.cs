using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term is required")
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters");
        
        RuleFor(x => x.Take)
            .GreaterThan(0).WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Take cannot exceed 100");
    }
}