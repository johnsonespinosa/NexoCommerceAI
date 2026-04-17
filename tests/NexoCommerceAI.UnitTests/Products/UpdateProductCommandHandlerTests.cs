using NexoCommerceAI.Application.Products.UpdateProduct;
using NexoCommerceAI.UnitTests.Products.TestHelpers;

namespace NexoCommerceAI.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        var productRepo = new InMemoryProductRepository();
        var categoryRepo = new InMemoryCategoryRepository();
        var handler = new UpdateProductCommandHandler(productRepo, categoryRepo);

        var command = new UpdateProductCommand(
            Id: Guid.NewGuid(),
            Name: "Updated",
            CategoryId: null,
            Description: null,
            Price: null,
            CompareAtPrice: null,
            Sku: null,
            Stock: null,
            IsFeatured: null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesProduct()
    {
        var product = NexoCommerceAI.Domain.Entities.Product.Create(
            name: "P1",
            categoryId: Guid.NewGuid(),
            description: null,
            price: 1m,
            compareAtPrice: null,
            sku: "SKU1",
            stock: 1,
            isFeatured: false);
        var productRepo = new InMemoryProductRepository();
        await productRepo.AddAsync(product);
        var categoryRepo = new InMemoryCategoryRepository(product.CategoryId);
        var handler = new UpdateProductCommandHandler(productRepo, categoryRepo);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Updated",
            CategoryId: product.CategoryId,
            Description: "Desc",
            Price: 2m,
            CompareAtPrice: null,
            Sku: "SKU2",
            Stock: 5,
            IsFeatured: true);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", product.Name);
        Assert.Equal(2m, product.Price);
    }
}
