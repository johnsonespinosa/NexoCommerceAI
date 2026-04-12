using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Carts.Handlers;

public class AddToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICartCacheService cartCacheService,
    ILogger<AddToCartCommandHandler> logger)
    : IRequestHandler<AddToCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product {ProductId} to cart for user {UserId}, Quantity: {Quantity}", 
            request.ProductId, request.UserId, request.Quantity);

        // Verificar que el producto existe y está disponible
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.ProductId);
        
        if (!product.IsActive || product.IsDeleted)
            throw new ValidationException($"Product '{product.Name}' is not available");
        
        if (product.Stock < request.Quantity)
            throw new ValidationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {request.Quantity}");
        
        // Obtener o crear el carrito
        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        
        if (cart == null)
        {
            cart = Cart.Create(request.UserId);
            await cartRepository.AddAsync(cart, cancellationToken);
        }
        
        // Agregar item al carrito
        cart.AddItem(product, request.Quantity, request.SelectedPrice);
        
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
        
        // Publicar eventos de dominio
        foreach (var domainEvent in cart.DomainEvents)
        {
            // Aquí se publicarían los eventos
        }
        cart.ClearDomainEvents();
        
        // Construir respuesta
        var response = CartResponse.MapToCartResponse(cart);
        
        // Actualizar caché
        await cartCacheService.SetCartAsync(request.UserId, response, cancellationToken: cancellationToken);
        
        logger.LogInformation("Product {ProductId} added to cart for user {UserId}. Cart total: {TotalAmount:C}", 
            request.ProductId, request.UserId, response.TotalAmount);
        
        return response;
    }
}