using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}