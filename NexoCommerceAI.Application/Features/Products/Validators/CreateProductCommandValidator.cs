using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");
        
        RuleFor(command => command.Slug)
            .MaximumLength(200).WithMessage("Slug must not exceed 200 characters")
            .MustAsync(async (slug, cancellation) => 
                string.IsNullOrWhiteSpace(slug) || await productRepository.IsSlugUniqueAsync(slug, cancellation))
            .WithMessage("Slug already exists");
        
        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
        
        RuleFor(command => command.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).WithMessage("Price cannot exceed 1,000,000");
        
        RuleFor(command => command.CompareAtPrice)
            .GreaterThan(0).When(command => command.CompareAtPrice.HasValue)
            .WithMessage("Compare at price must be greater than 0")
            .LessThanOrEqualTo(1_000_000).When(command => command.CompareAtPrice.HasValue)
            .WithMessage("Compare at price cannot exceed 1,000,000")
            .Must((command, compareAtPrice) => 
                !compareAtPrice.HasValue || compareAtPrice.Value > command.Price)
            .WithMessage("Compare at price must be greater than regular price");
        
        RuleFor(command => command.Sku)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .MustAsync(async (sku, cancellation) => 
                string.IsNullOrWhiteSpace(sku) || !await productRepository.ExistBySkuAsync(sku, cancellation))
            .WithMessage("SKU already exists");
        
        RuleFor(command => command.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
        
        RuleFor(command => command.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");
        
        // Validación adicional: La categoría debe existir y estar activa
        RuleFor(command => command.CategoryId)
            .NotEmpty()
            .MustAsync(async (categoryId, cancellation) =>
            {
                var category = await categoryRepository.GetByIdAsync(categoryId, cancellation);
                return category is { IsActive: true, IsDeleted: false };
            })
            .WithMessage("Category must exist and be active");
        
        // Validación: El precio de oferta no puede ser mayor que el precio original * 2
        RuleFor(command => command.CompareAtPrice)
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || compareAtPrice.Value <= command.Price * 2)
            .WithMessage("Compare at price cannot be more than double the regular price");
        
        // Validación: El nombre no puede tener caracteres especiales no permitidos
        RuleFor(command => command.Name)
            .Must(name => !System.Text.RegularExpressions.Regex.IsMatch(name, @"[<>""'%&]"))
            .WithMessage("Name contains invalid characters");
        
        RuleFor(command => command.CompareAtPrice)
            .Must((command, compareAtPrice) =>
                !compareAtPrice.HasValue || compareAtPrice.Value >= command.Price * 1.1m)
            .When(command => command.CompareAtPrice.HasValue)
            .WithMessage("Compare at price should be at least 10% higher than regular price for a meaningful discount");
        
        RuleFor(command => command.Slug)
            .Must(slug => string.IsNullOrWhiteSpace(slug) || 
                          System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");
        
        RuleFor(command => command.Stock)
            .LessThanOrEqualTo(1000).WithMessage("Initial stock seems high, please verify");
        
        RuleFor(command => command.Name)
            .MustAsync(async (name, cancellation) =>
                !await productRepository.ExistByNameAsync(name, cancellation))
            .WithMessage("Product name already exists");
        
        RuleFor(command => command.Price)
            .Must(price => Math.Round(price, 2) == price)
            .WithMessage("Price must have at most 2 decimal places");
    }
}