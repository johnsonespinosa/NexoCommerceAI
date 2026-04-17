using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.CreateProduct;

internal static class CreateProductErrors
{
    public static Error DuplicateSku(string sku) =>
        ProductErrors.DuplicateSku(sku);

    public static Error InvalidRequest(string detail) =>
        ProductErrors.InvalidRequest(detail);
}
