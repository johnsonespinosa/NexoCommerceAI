using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class UpdateProductPriceCommandHandler(
    IProductRepository productRepository,
    ILogger<UpdateProductPriceCommandHandler> logger)
    : IRequestHandler<UpdateProductPriceCommand, bool>
{
    public async Task<bool> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.UpdatePriceStarted(
                request.ProductId, 
                request.NewPrice, 
                request.NewCompareAtPrice?.ToString() ?? "none");
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                throw new NotFoundException(nameof(Product), request.ProductId);
            }
            
            var oldPrice = product.Price;
            var oldCompareAtPrice = product.CompareAtPrice;
            
            // Validar reglas de negocio antes de actualizar
            ValidatePriceRules(request, product);
            
            product.UpdatePrice(request.NewPrice, request.NewCompareAtPrice);
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.PriceUpdated(
                request.ProductId, 
                oldPrice, 
                request.NewPrice, 
                oldCompareAtPrice, 
                request.NewCompareAtPrice);
            
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid price data for product: {ProductId}, Error: {Message}", 
                request.ProductId, ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating price for product: {ProductId}", request.ProductId);
            throw;
        }
    }
    
    private static void ValidatePriceRules(UpdateProductPriceCommand request, Product product)
    {
        // Validar que el precio de oferta no sea más del doble
        if (request.NewCompareAtPrice.HasValue && request.NewCompareAtPrice.Value > request.NewPrice * 2)
            throw new ValidationException("Compare at price cannot be more than double the regular price");
        
        // Validar que el precio de oferta sea al menos 10% más alto (para descuento significativo)
        if (request.NewCompareAtPrice.HasValue && request.NewCompareAtPrice.Value < request.NewPrice * 1.1m)
            throw new ValidationException("Compare at price should be at least 10% higher than regular price for a meaningful discount");
        
        // Validar que el precio de oferta sea mayor que el precio regular
        if (request.NewCompareAtPrice.HasValue && request.NewCompareAtPrice.Value <= request.NewPrice)
            throw new ValidationException("Compare at price must be greater than regular price");
    }
}