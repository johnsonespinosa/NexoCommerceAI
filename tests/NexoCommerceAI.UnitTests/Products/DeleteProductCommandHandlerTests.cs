using NexoCommerceAI.Application.Products.DeleteProduct;
using NexoCommerceAI.UnitTests.Products.TestHelpers;

namespace NexoCommerceAI.UnitTests.Products;

public class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductExists_SoftDeletes()
    {
        var product = NexoCommerceAI.Domain.Entities.Product.Create("P1", Guid.NewGuid(), null, description: null, price: 1m, compareAtPrice: null, sku: "SKU1", stock: 1, isFeatured: false);
        var productRepo = new InMemoryProductRepository();
        await productRepo.AddAsync(product);

        var handler = new DeleteProductCommandHandler(productRepo);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(product.IsDeleted);
    }
}
