using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.ProductUpdateStarted(request.Id);
            
            // Verificar que al menos un campo se está actualizando
            if (!HasAnyUpdate(request))
                throw new ValidationException("At least one field must be provided for update");
            
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), request.Id);
            
            logger.LogDebug(
                "Current product values - Name: {Name}, Price: {Price}, Stock: {Stock}, Sku: {Sku}",
                product.Name, product.Price, product.Stock, product.Sku);
            
            // Verificar categoría si se está actualizando
            Category? newCategory = null;
            if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
            {
                newCategory = await categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
                if (newCategory == null || newCategory.IsDeleted || !newCategory.IsActive)
                    throw new NotFoundException(nameof(Category), request.CategoryId.Value);
                
                logger.LogInformation("Changing product category from {OldCategoryId} to {NewCategoryId} - {CategoryName}", 
                    product.CategoryId, newCategory.Id, newCategory.Name);
            }
            
            // Validaciones de negocio para precios
            ValidatePriceRules(request, product);
            
            // Actualizar producto
            product.Update(
                request.Name!,
                request.Slug,
                request.Description,
                request.Price,
                request.CompareAtPrice,
                request.Sku,
                request.Stock,
                request.IsFeatured,
                request.CategoryId
            );
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.ProductUpdated(product.Id, product.Name);
            
            // Obtener categoría final para la respuesta
            var finalCategory = newCategory ?? await categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);
            var response = new ProductResponse(product, finalCategory);
            
            return response;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid product data for update: {ProductId}, Error: {Message}", 
                request.Id, ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating product: {ProductId}", request.Id);
            throw;
        }
    }
    
    private static bool HasAnyUpdate(UpdateProductCommand request)
    {
        return request.Name != null ||
               request.Slug != null ||
               request.Description != null ||
               request.Price.HasValue ||
               request.CompareAtPrice.HasValue ||
               request.Sku != null ||
               request.Stock.HasValue ||
               request.IsFeatured.HasValue ||
               request.CategoryId.HasValue;
    }
    
    private static void ValidatePriceRules(UpdateProductCommand request, Product product)
    {
        var finalPrice = request.Price ?? product.Price;
        var finalCompareAtPrice = request.CompareAtPrice ?? product.CompareAtPrice;
        
        if (finalCompareAtPrice.HasValue)
        {
            if (finalCompareAtPrice.Value > finalPrice * 2)
                throw new ValidationException("Compare at price cannot be more than double the regular price");
            
            if (finalCompareAtPrice.Value < finalPrice * 1.1m)
                throw new ValidationException("Compare at price should be at least 10% higher than regular price for a meaningful discount");
        }
    }
}