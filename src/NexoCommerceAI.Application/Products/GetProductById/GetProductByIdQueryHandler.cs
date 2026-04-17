using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Id));
        }

        return Result.Success(product.ToResponse());
    }
}
