using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class AddProductImageCommandHandler(
    IProductRepository productRepository,
    IImageStorageService imageStorageService,
    ILogger<AddProductImageCommandHandler> logger)
    : IRequestHandler<AddProductImageCommand, ProductImageResponse>
{
    public async Task<ProductImageResponse> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Adding image to product: {ProductId}", request.ProductId);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), request.ProductId);
            
            // Validar tipo de archivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                throw new ValidationException($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
            
            // Validar tamaño máximo (5MB)
            if (request.Image.Length > 5 * 1024 * 1024)
                throw new ValidationException("Image size cannot exceed 5MB");
            
            // Subir imagen a Cloudinary/S3
            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await imageStorageService.UploadImageAsync(
                stream,
                $"{product.Slug}_{Guid.NewGuid()}{extension}",
                $"products/{product.Id}",
                cancellationToken);
            
            // Crear entidad ProductImage usando el factory method
            var productImage = ProductImage.Create(
                productId: product.Id,
                imageUrl: uploadResult.Url,
                publicId: uploadResult.PublicId,
                isMain: request.IsMain,
                displayOrder: request.DisplayOrder);
            
            // Usar el método de negocio de Product para agregar la imagen
            product.AddImage(productImage);
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Image added successfully to product {ProductId}: {ImageUrl}", 
                product.Id, productImage.ImageUrl);
            
            return new ProductImageResponse
            {
                Id = productImage.Id,
                ProductId = productImage.ProductId,
                ImageUrl = productImage.ImageUrl,
                IsMain = productImage.IsMain,
                DisplayOrder = productImage.DisplayOrder,
                CreatedAt = productImage.CreatedAt
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding image to product {ProductId}", request.ProductId);
            throw;
        }
    }
}