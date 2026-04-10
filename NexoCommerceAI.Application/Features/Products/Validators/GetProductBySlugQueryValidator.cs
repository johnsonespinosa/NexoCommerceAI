using System;
using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetProductBySlugQueryValidator : AbstractValidator<GetProductBySlugQuery>
{
    public GetProductBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");
    }
}
