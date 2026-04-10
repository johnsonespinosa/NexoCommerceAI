using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    public GetProductsByCategoryQueryValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required")
            .MustAsync(async (categoryId, cancellation) =>
                await categoryRepository.ExistsAsync(categoryId, cancellation))
            .WithMessage("Category does not exist");
        
        RuleFor(x => x.Take)
            .GreaterThan(0).When(x => x.Take.HasValue)
            .WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(100).When(x => x.Take.HasValue)
            .WithMessage("Take cannot exceed 100");
    }
}