using FluentValidation;

namespace NexoCommerceAI.Application.Products.GetProducts;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize must be at least 1")
            .LessThanOrEqualTo(100).WithMessage("PageSize must be at most 100");
    }
}
