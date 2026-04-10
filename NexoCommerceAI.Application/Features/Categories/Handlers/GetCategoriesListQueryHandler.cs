using MediatR;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Categories.Queries;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesListQuery, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.ListAsync(cancellationToken);
        return categories.Select(category =>
            new CategoryResponse(
                category.Id,
                category.Name,
                category.Slug,
                category.IsActive,
                category.CreatedAt))
            .ToList();
    }
}