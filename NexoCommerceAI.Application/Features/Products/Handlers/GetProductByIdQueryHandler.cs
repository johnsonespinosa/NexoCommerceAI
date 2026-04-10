using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductByIdQueryHandler> logger) 
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetProductByIdStarted(request.Id);
            
            var product = await productRepository.GetByIdWithCategoryAsync(request.Id, cancellationToken);
            
            if (product == null)
            {
                logger.ProductNotFound(request.Id);
                return null;
            }
            
            logger.ProductFound(product.Id, product.Name, product.Category.Name);
            
            var response = new ProductResponse(product, product.Category);
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting product by ID: {ProductId}", request.Id);
            throw;
        }
    }
}