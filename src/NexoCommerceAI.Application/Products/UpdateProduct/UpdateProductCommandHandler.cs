using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : ICommandHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.Id));
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
        {
            if (!await categoryRepository.ExistsAsync(request.CategoryId.Value, cancellationToken))
            {
                return Result.Failure<ProductResponse>(ProductErrors.CategoryNotFound(request.CategoryId.Value));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var skuExists = await productRepository.ExistsBySkuAsync(
                request.Sku,
                excludingProductId: request.Id,
                cancellationToken);

            if (skuExists)
            {
                return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku(request.Sku));
            }
        }

        try
        {
            product.Update(
                name: request.Name,
                categoryId: request.CategoryId,
                description: request.Description,
                price: request.Price,
                compareAtPrice: request.CompareAtPrice,
                sku: request.Sku,
                stock: request.Stock,
                isFeatured: request.IsFeatured);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ProductResponse>(ProductErrors.InvalidRequest(ex.Message));
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(product.ToResponse());
    }
}
