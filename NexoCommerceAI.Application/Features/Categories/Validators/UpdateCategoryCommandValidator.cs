using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;

namespace NexoCommerceAI.Application.Features.Categories.Validators;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        var categoryRepository1 = categoryRepository;
        
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required");
        
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => x.Name != null)
            .WithMessage("Category name must not exceed 100 characters")
            .Must(name => string.IsNullOrWhiteSpace(name) || 
                          !System.Text.RegularExpressions.Regex.IsMatch(name, @"[<>""'%&]"))
            .WithMessage("Category name contains invalid characters")
            .MustAsync(async (command, name, cancellation) =>
                string.IsNullOrWhiteSpace(name) ||
                await categoryRepository1.IsNameUniqueAsync(name, command.Id, cancellation))
            .WithMessage("Category name already exists");
        
        RuleFor(x => x.Slug)
            .MaximumLength(100).When(x => x.Slug != null)
            .WithMessage("Slug must not exceed 100 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .MustAsync(async (command, slug, cancellation) =>
                string.IsNullOrWhiteSpace(slug) ||
                await categoryRepository1.IsSlugUniqueAsync(slug, command.Id, cancellation))
            .WithMessage("Slug already exists");
    }
}