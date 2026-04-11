using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class RemoveProductImageCommandHandler(
    IProductRepository productRepository,
    IImageStorageService imageStorageService,
    ILogger<RemoveProductImageCommandHandler> logger)
    : IRequestHandler<RemoveProductImageCommand, bool>
{
    public async Task<bool> Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Removing image {ImageId} from product {ProductId}", 
                request.ImageId, request.ProductId);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), request.ProductId);
            
            var image = product.Images.FirstOrDefault(i => i.Id == request.ImageId);
            if (image == null)
                throw new NotFoundException(nameof(ProductImage), request.ImageId);
            
            // Eliminar de Cloudinary/S3
            await imageStorageService.DeleteImageAsync(image.PublicId, cancellationToken);
            
            // Usar el método de negocio de Product para eliminar la imagen
            product.RemoveImage(image);
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Image {ImageId} removed successfully from product {ProductId}", 
                request.ImageId, request.ProductId);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing image {ImageId} from product {ProductId}", 
                request.ImageId, request.ProductId);
            throw;
        }
    }
}