using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.ProductCreationStarted(request.Name, request.CategoryId, request.Price, request.Stock);

            // Verificar que la categoría existe
            var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                throw new NotFoundException(nameof(Category), request.CategoryId);

            if (request.CompareAtPrice.HasValue && request.CompareAtPrice.Value > request.Price * 2)
                throw new ValidationException("Compare at price cannot be more than double the regular price");

            // Verificar unicidad de SKU si se proporcionó
            if (!string.IsNullOrWhiteSpace(request.Sku))
            {
                var skuExists = await productRepository.ExistBySkuAsync(request.Sku, cancellationToken);
                if (skuExists)
                    throw new ValidationException($"SKU '{request.Sku}' already exists");
            }

            // Verificar unicidad de Slug si se proporcionó
            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                var slugExists = await productRepository.ExistBySlugAsync(request.Slug, cancellationToken);
                if (slugExists)
                    throw new ValidationException($"Slug '{request.Slug}' already exists");
            }

            // Crear el producto
            var product = Product.Create(
                request.Name,
                request.CategoryId,
                request.Slug,
                request.Description,
                request.Price,
                request.CompareAtPrice,
                request.Sku,
                request.Stock,
                request.IsFeatured
            );

            if (string.IsNullOrWhiteSpace(product.Slug))
            {
                logger.LogWarning("Generated slug is empty for product: {ProductName}", product.Name);
                throw new ValidationException("Unable to generate valid slug from product name");
            }

            await productRepository.AddAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);

            logger.ProductCreated(product.Id, product.Name);

            var response = new ProductResponse(product, category);

            return response;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid product data: {Message}", ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating product: {ProductName}", request.Name);
            throw;
        }
    }
}