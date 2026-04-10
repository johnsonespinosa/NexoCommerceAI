using FluentValidation;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class GetProductsListQueryValidator : AbstractValidator<GetProductsListQuery>
{
    public GetProductsListQueryValidator()
    {
        RuleFor(x => x.Pagination.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");
        
        RuleFor(x => x.Pagination.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Page size cannot exceed 50");
        
        RuleFor(x => x.Pagination.MinPrice)
            .GreaterThan(0).When(x => x.Pagination.MinPrice.HasValue)
            .WithMessage("Minimum price must be greater than 0");
        
        RuleFor(x => x.Pagination.MaxPrice)
            .GreaterThan(0).When(x => x.Pagination.MaxPrice.HasValue)
            .WithMessage("Maximum price must be greater than 0")
            .Must((query, maxPrice) => 
                !maxPrice.HasValue || 
                !query.Pagination.MinPrice.HasValue || 
                maxPrice.Value >= query.Pagination.MinPrice.Value)
            .WithMessage("Maximum price must be greater than or equal to minimum price");
        
        RuleFor(x => x.Pagination.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || 
                            new[] { "name", "price", "createdat", "stock" }.Contains(sortBy.ToLower()))
            .WithMessage("Sort by must be one of: name, price, createdAt, stock");
    }
}