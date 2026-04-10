using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;

namespace NexoCommerceAI.Application.Features.Categories.Validators;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .Must(name => !System.Text.RegularExpressions.Regex.IsMatch(name, @"[<>""'%&]"))
            .WithMessage("Category name contains invalid characters");
        
        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .MustAsync(async (slug, cancellation) => 
                string.IsNullOrWhiteSpace(slug) || await categoryRepository.IsSlugUniqueAsync(slug, cancellation))
            .WithMessage("Slug already exists");
    }
}