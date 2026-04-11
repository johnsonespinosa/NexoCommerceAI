using FluentValidation;
using Microsoft.AspNetCore.Http;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
        
        RuleFor(x => x.Image)
            .NotNull().WithMessage("Image file is required")
            .Must(BeValidImageSize).WithMessage("Image size cannot exceed 5MB")
            .Must(BeValidImageType).WithMessage("Invalid file type. Allowed types: .jpg, .jpeg, .png, .webp, .gif");
        
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).When(x => x.DisplayOrder.HasValue)
            .WithMessage("Display order cannot be negative");
    }
    
    private static bool BeValidImageSize(IFormFile? file)
    {
        if (file == null) return false;
        return file.Length <= 5 * 1024 * 1024; // 5MB
    }
    
    private static bool BeValidImageType(IFormFile? file)
    {
        if (file == null) return false;
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}