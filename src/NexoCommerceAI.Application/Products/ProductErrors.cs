using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products;

internal static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Products.NotFound", $"Product '{id}' was not found.");

    public static Error DuplicateSku(string sku) =>
        Error.Conflict("Products.DuplicateSku", $"SKU '{sku}' already exists.");

    public static Error CategoryNotFound(Guid id) =>
        Error.NotFound("Products.CategoryNotFound", $"Category '{id}' was not found.");

    public static Error InvalidRequest(string detail) =>
        Error.Validation("Products.InvalidRequest", detail);
}
