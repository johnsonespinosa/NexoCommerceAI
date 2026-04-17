using System.ComponentModel.DataAnnotations;

namespace NexoCommerceAI.API.Contracts.Products;

public sealed record UpdateProductRequest(
    [property: MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
    [property: MaxLength(120, ErrorMessage = "Name must be 120 characters or fewer.")]
    string? Name,

    Guid? CategoryId,

    [property: MaxLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    string? Description,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    decimal? Price,

    [property: Range(0.0, double.MaxValue, ErrorMessage = "CompareAtPrice must be zero or greater.")]
    decimal? CompareAtPrice,

    [property: MinLength(1, ErrorMessage = "Sku must be at least 1 character.")]
    [property: MaxLength(50, ErrorMessage = "Sku must be 50 characters or fewer.")]
    string? Sku,

    [property: Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    int? Stock,

    bool? IsFeatured);
