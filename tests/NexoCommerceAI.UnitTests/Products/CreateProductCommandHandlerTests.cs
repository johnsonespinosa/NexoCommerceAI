using NexoCommerceAI.Application.Products.CreateProduct;
using NexoCommerceAI.UnitTests.Products.TestHelpers;

namespace NexoCommerceAI.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCategoryDoesNotExist_ReturnsFailure()
    {
        var productRepo = new InMemoryProductRepository();
        var categoryRepo = new InMemoryCategoryRepository();
        var handler = new CreateProductCommandHandler(productRepo, categoryRepo);

        var command = new CreateProductCommand(
            Name: "Test",
            CategoryId: Guid.NewGuid(),
            Description: null,
            Price: 10m,
            CompareAtPrice: null,
            Sku: null,
            Stock: 1,
            IsFeatured: false);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WhenSkuDuplicate_ReturnsFailure()
    {
        var productRepo = new InMemoryProductRepository();
        var existing = NexoCommerceAI.Domain.Entities.Product.Create(
            name: "P1",
            categoryId: Guid.NewGuid(),
            description: null,
            price: 1m,
            compareAtPrice: null,
            sku: "SKU1",
            stock: 1,
            isFeatured: false);
        await productRepo.AddAsync(existing);
        var categoryRepo = new InMemoryCategoryRepository(existing.CategoryId);

        var handler = new CreateProductCommandHandler(productRepo, categoryRepo);

        var command = new CreateProductCommand(
            Name: "Test",
            CategoryId: existing.CategoryId,
            Description: null,
            Price: 10m,
            CompareAtPrice: null,
            Sku: "SKU1",
            Stock: 1,
            IsFeatured: false);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
