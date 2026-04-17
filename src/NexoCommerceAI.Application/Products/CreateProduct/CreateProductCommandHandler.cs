using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<Result<CreateProductResult>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            return Result.Failure<CreateProductResult>(ProductErrors.CategoryNotFound(request.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var skuExists = await productRepository.ExistsBySkuAsync(request.Sku, cancellationToken: cancellationToken);
            if (skuExists)
            {
                return Result.Failure<CreateProductResult>(CreateProductErrors.DuplicateSku(request.Sku));
            }
        }

        Product product;
        try
        {
            product = Product.Create(
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
            return Result.Failure<CreateProductResult>(CreateProductErrors.InvalidRequest(ex.Message));
        }

        await productRepository.AddAsync(product, cancellationToken);

        return Result.Success(new CreateProductResult(
            Id: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            Sku: product.Sku,
            Price: product.Price,
            Stock: product.Stock,
            CategoryId: product.CategoryId));
    }
}
