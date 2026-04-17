using FluentValidation;

namespace NexoCommerceAI.Application.Products.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue)
            .WithMessage("CompareAtPrice must be greater or equal to 0");

        RuleFor(x => x.Sku)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Sku))
            .WithMessage("Sku must be at most 100 characters");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater or equal to 0");
    }
}
