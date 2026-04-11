using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Commands;

/// <summary>
/// Command para actualizar un producto existente
/// </summary>
/// <param name="Id">Identificador único del producto</param>
/// <param name="Name">Nuevo nombre del producto (opcional)</param>
/// <param name="Slug">Nuevo slug SEO-friendly (opcional)</param>
/// <param name="Description">Nueva descripción (opcional)</param>
/// <param name="Price">Nuevo precio (opcional)</param>
/// <param name="CompareAtPrice">Nuevo precio de comparación (opcional)</param>
/// <param name="Sku">Nuevo SKU (opcional)</param>
/// <param name="Stock">Nuevo stock (opcional)</param>
/// <param name="IsFeatured">Indica si el producto es destacado (opcional)</param>
/// <param name="CategoryId">Nueva categoría (opcional)</param>
[InvalidateCache("products_list", "featured_products", "products_on_sale", "product_by_id")]
public record UpdateProductCommand(
    Guid Id,
    string? Name = null,
    string? Slug = null,
    string? Description = null,
    decimal? Price = null,
    decimal? CompareAtPrice = null,
    string? Sku = null,
    int? Stock = null,
    bool? IsFeatured = null,
    Guid? CategoryId = null) : IRequest<ProductResponse>;