using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class SetMainImageCommandHandler(
    IProductRepository productRepository,
    ILogger<SetMainImageCommandHandler> logger)
    : IRequestHandler<SetMainImageCommand, bool>
{
    public async Task<bool> Handle(SetMainImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Setting main image {ImageId} for product {ProductId}", 
                request.ImageId, request.ProductId);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), request.ProductId);
            
            // Usar el método de negocio de Product para establecer la imagen principal
            product.SetMainImage(request.ImageId);
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Image {ImageId} set as main for product {ProductId}", 
                request.ImageId, request.ProductId);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting main image {ImageId} for product {ProductId}", 
                request.ImageId, request.ProductId);
            throw;
        }
    }
}