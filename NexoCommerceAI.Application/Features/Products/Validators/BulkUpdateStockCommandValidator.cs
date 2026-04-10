using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class BulkUpdateStockCommandValidator : AbstractValidator<BulkUpdateStockCommand>
{
    public BulkUpdateStockCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required for bulk update")
            .Must(items => items.Count <= 1000).WithMessage("Cannot update more than 1000 items at once");
        
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEmpty().WithMessage("Product ID is required");
                
                item.RuleFor(i => i.NewStock)
                    .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
                    .LessThanOrEqualTo(999999).WithMessage("Stock cannot exceed 999,999");
            });
        
        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate product IDs found in the update list");
    }
}