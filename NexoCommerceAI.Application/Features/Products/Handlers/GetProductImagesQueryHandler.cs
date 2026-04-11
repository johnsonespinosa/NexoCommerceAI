using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductImagesQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductImagesQueryHandler> logger)
    : IRequestHandler<GetProductImagesQuery, IReadOnlyList<ProductImageResponse>>
{
    public async Task<IReadOnlyList<ProductImageResponse>> Handle(GetProductImagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting images for product: {ProductId}", request.ProductId);
            
            // Primero verificar que el producto existe
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                return new List<ProductImageResponse>();
            }
            
            // Obtener las imágenes del producto
            var images = product.Images
                .Where(i => i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .ThenByDescending(i => i.IsMain)
                .Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl,
                    PublicId = i.PublicId,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder,
                    CreatedAt = i.CreatedAt
                })
                .ToList();
            
            logger.LogDebug("Found {Count} images for product {ProductId}", images.Count, request.ProductId);
            
            return images;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting images for product {ProductId}", request.ProductId);
            throw;
        }
    }
}