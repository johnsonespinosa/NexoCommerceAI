using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class CategoryRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CategoryRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategoryToDatabase()
    {
        // Arrange
        var category = Category.Create("New Category", "new-category");

        // Act
        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();

        // Assert
        var savedCategory = await _fixture.Context.Categories.FindAsync(category.Id);
        Assert.NotNull(savedCategory);
        Assert.Equal(category.Name, savedCategory.Name);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCategoryWhenExists()
    {
        // Act
        var result = await _repository.GetBySlugAsync("electronics");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNullWhenNotExists()
    {
        // Act
        var result = await _repository.GetBySlugAsync("non-existent-slug");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsSlugUniqueAsync_WithSingleParameter_ShouldReturnTrueForUniqueSlug()
    {
        // Act
        var isUnique = await _repository.IsSlugUniqueAsync("unique-slug", null);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public async Task IsSlugUniqueAsync_WithSingleParameter_ShouldReturnFalseForExistingSlug()
    {
        // Act
        var isUnique = await _repository.IsSlugUniqueAsync("electronics", null);

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public async Task IsSlugUniqueAsync_WithExcludeId_ShouldReturnTrueWhenUpdatingToSameSlug()
    {
        // Arrange
        var category = await _fixture.Context.Categories.FirstAsync(c => c.Slug == "electronics");
        
        // Act - Excluyendo la categoría actual, el slug "electronics" debería ser considerado único
        var isUnique = await _repository.IsSlugUniqueAsync("electronics", category.Id);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public async Task IsSlugUniqueAsync_WithExcludeId_ShouldReturnFalseForDifferentCategoryWithSameSlug()
    {
        // Arrange
        var category = await _fixture.Context.Categories.FirstAsync(c => c.Slug == "electronics");
        var otherCategoryId = Guid.NewGuid();
        
        // Act - Excluyendo una categoría diferente, el slug "electronics" no debería ser único
        var isUnique = await _repository.IsSlugUniqueAsync("electronics", otherCategoryId);

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public async Task IsNameUniqueAsync_ShouldReturnTrueForUniqueName()
    {
        // Act
        var isUnique = await _repository.IsNameUniqueAsync("Unique Category Name");

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public async Task IsNameUniqueAsync_ShouldReturnFalseForExistingName()
    {
        // Act
        var isUnique = await _repository.IsNameUniqueAsync("Electronics");

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public async Task IsNameUniqueAsync_WithExcludeId_ShouldReturnTrueWhenUpdatingToSameName()
    {
        // Arrange
        var category = await _fixture.Context.Categories.FirstAsync(c => c.Name == "Electronics");
        
        // Act - Excluyendo la categoría actual, el nombre "Electronics" debería ser considerado único
        var isUnique = await _repository.IsNameUniqueAsync("Electronics", category.Id);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
    {
        // Arrange
        var category = await _fixture.Context.Categories.FirstAsync();

        // Act
        var exists = await _repository.ExistsAsync(category.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetActiveCategoriesAsync_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var inactiveCategory = await _fixture.Context.Categories.FirstAsync();
        inactiveCategory.Deactivate();
        await _fixture.Context.SaveChangesAsync();

        // Act
        var activeCategories = await _repository.GetActiveCategoriesAsync();

        // Assert
        Assert.DoesNotContain(activeCategories, c => c.Id == inactiveCategory.Id);
        Assert.All(activeCategories, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllNonDeletedCategories()
    {
        // Act
        var categories = await _repository.ListAsync();

        // Assert
        Assert.NotNull(categories);
        Assert.All(categories, c => Assert.False(c.IsDeleted));
    }
}