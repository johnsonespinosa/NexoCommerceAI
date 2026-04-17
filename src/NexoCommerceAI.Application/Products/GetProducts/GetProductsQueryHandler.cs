using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.GetProducts;

public sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, PaginatedResult<ProductResponse>>
{
    public async Task<Result<PaginatedResult<ProductResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.CategoryId,
            request.Sku,
            request.IsFeatured,
            cancellationToken);

        var response = products.Items.Select(product => product.ToResponse()).ToList();
        var paginatedResponse = new PaginatedResult<ProductResponse>(response, products.Page, products.PageSize, products.TotalCount);

        return Result.Success(paginatedResponse);
    }
}
