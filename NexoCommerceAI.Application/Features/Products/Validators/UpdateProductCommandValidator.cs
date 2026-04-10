using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository)  // Agregar categoryRepository
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
        
        RuleFor(x => x.Name)
            .MaximumLength(200).When(x => x.Name != null)
            .WithMessage("Product name must not exceed 200 characters")
            .Must(name => string.IsNullOrWhiteSpace(name) || 
                !System.Text.RegularExpressions.Regex.IsMatch(name, @"[<>""'%&]"))
            .WithMessage("Name contains invalid characters");
        
        RuleFor(x => x.Slug)
            .MaximumLength(200).When(x => x.Slug != null)
            .WithMessage("Slug must not exceed 200 characters")
            .Must(slug => string.IsNullOrWhiteSpace(slug) || 
                System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
            .MustAsync(async (command, slug, cancellation) => 
                string.IsNullOrWhiteSpace(slug) || 
                await productRepository.IsSlugUniqueAsync(slug, cancellation) ||
                (await productRepository.GetByIdAsync(command.Id, cancellation))?.Slug == slug)
            .WithMessage("Slug already exists");
        
        RuleFor(x => x.Description)
            .MaximumLength(2000).When(x => x.Description != null)
            .WithMessage("Description must not exceed 2000 characters");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).When(x => x.Price.HasValue)
            .WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).When(x => x.Price.HasValue)
            .WithMessage("Price cannot exceed 1,000,000")
            .Must(price => !price.HasValue || Math.Round(price.Value, 2) == price.Value)
            .WithMessage("Price must have at most 2 decimal places");
        
        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0).When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare at price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).When(x => x.CompareAtPrice.HasValue)
            .WithMessage("Compare at price cannot exceed 1,000,000")
            .Must((command, compareAtPrice) => 
                !compareAtPrice.HasValue || 
                !command.Price.HasValue || 
                compareAtPrice.Value > command.Price.Value)
            .WithMessage("Compare at price must be greater than regular price")
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || 
                !command.Price.HasValue || 
                compareAtPrice.Value <= command.Price.Value * 2)
            .WithMessage("Compare at price cannot be more than double the regular price")
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || 
                !command.Price.HasValue || 
                compareAtPrice.Value >= command.Price.Value * 1.1m)
            .When(x => x.CompareAtPrice.HasValue && x.Price.HasValue)
            .WithMessage("Compare at price should be at least 10% higher than regular price for a meaningful discount");
        
        RuleFor(x => x.Sku)
            .MaximumLength(50).When(x => x.Sku != null)
            .WithMessage("SKU must not exceed 50 characters")
            .MustAsync(async (command, sku, cancellation) => 
                string.IsNullOrWhiteSpace(sku) || 
                !await productRepository.ExistBySkuAsync(sku, cancellation) ||
                (await productRepository.GetByIdAsync(command.Id, cancellation))?.Sku == sku)
            .WithMessage("SKU already exists");
        
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).When(x => x.Stock.HasValue)
            .WithMessage("Stock cannot be negative");
        
        // Validación de categoría
        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellation) =>
                !categoryId.HasValue || 
                await categoryRepository.GetByIdAsync(categoryId.Value, cancellation) is { IsActive: true, IsDeleted: false })
            .When(x => x.CategoryId.HasValue)
            .WithMessage("Category must exist and be active");
    }
}