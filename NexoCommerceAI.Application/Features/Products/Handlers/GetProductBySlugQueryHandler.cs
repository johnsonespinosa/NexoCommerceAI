using System;
using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductBySlugQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductBySlugQueryHandler> logger)
    : IRequestHandler<GetProductBySlugQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetProductBySlugStarted(request.Slug);
            
            var product = await productRepository.GetBySlugAsync(request.Slug, cancellationToken);
            
            if (product == null)
            {
                logger.ProductNotFoundBySlug(request.Slug);
                return null;
            }
            
            logger.ProductFoundBySlug(request.Slug, product.Id, product.Name);
            
            var response = new ProductResponse(product, product.Category);
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting product by slug: {Slug}", request.Slug);
            throw;
        }
    }
}
