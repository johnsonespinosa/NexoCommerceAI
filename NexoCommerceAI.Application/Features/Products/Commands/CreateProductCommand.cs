using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Commands;

/// <summary>
/// Command para crear un nuevo producto
/// </summary>
/// <param name="Name">Nombre del producto</param>
/// <param name="Slug">Slug SEO-friendly (opcional, se genera automáticamente)</param>
/// <param name="Description">Descripción del producto (opcional)</param>
/// <param name="Price">Precio del producto</param>
/// <param name="CompareAtPrice">Precio de comparación (para ofertas)</param>
/// <param name="Sku">SKU del producto (opcional, se genera automáticamente)</param>
/// <param name="Stock">Cantidad en inventario</param>
/// <param name="IsFeatured">Indica si el producto es destacado</param>
/// <param name="CategoryId">Identificador de la categoría</param>
[InvalidateCache("products_list", "featured_products", "products_on_sale")]
public record CreateProductCommand(
    string Name,
    string? Slug = null,
    string? Description = null,
    decimal Price = 0,
    decimal? CompareAtPrice = null,
    string? Sku = null,
    int Stock = 0,
    bool IsFeatured = false,
    Guid CategoryId = default) : IRequest<ProductResponse>;