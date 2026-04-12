using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.ValueObjects;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for user {UserId}", request.UserId);
        
        // Obtener carrito del usuario
        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        if (cart == null || !cart.Items.Any())
            throw new ValidationException("Cart is empty. Cannot create order.");
        
        // Crear dirección de envío
        var shippingAddress = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State ?? string.Empty,
            request.ShippingAddress.ZipCode,
            request.ShippingAddress.Country);
        
        // Crear dirección de facturación
        var billingAddress = new Address(
            request.BillingAddress.Street,
            request.BillingAddress.City,
            request.BillingAddress.State ?? string.Empty,
            request.BillingAddress.ZipCode,
            request.BillingAddress.Country);
        
        // Crear la orden
        var order = Order.Create(request.UserId, shippingAddress, billingAddress, request.CustomerNotes);
        
        // Agregar items al pedido
        foreach (var cartItem in cart.Items)
        {
            var product = await productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException(nameof(Product), cartItem.ProductId);
            
            order.AddItem(product, cartItem.Quantity, cartItem.UnitPrice);
        }
        
        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        // Limpiar carrito después de crear la orden
        cart.Clear();
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order created successfully: {OrderNumber} for user {UserId}", 
            order.OrderNumber, request.UserId);
        
        return OrderResponse.MapToOrderResponse(order);
    }
    
    
}