using FluentValidation;

namespace NexoCommerceAI.Application.Products.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Name)
            .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be a valid GUID");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).When(x => x.Price.HasValue)
            .WithMessage("Price must be greater or equal to 0");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue)
            .WithMessage("CompareAtPrice must be greater or equal to 0");

        RuleFor(x => x.Sku)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Sku))
            .WithMessage("Sku must be at most 100 characters");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).When(x => x.Stock.HasValue)
            .WithMessage("Stock must be greater or equal to 0");
    }
}
