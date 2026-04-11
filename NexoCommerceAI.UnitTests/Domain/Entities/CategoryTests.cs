using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class CategoryTests
{
    private readonly Guid _categoryId = Guid.NewGuid();
    
    [Fact]
    public void Create_WithNameOnly_ShouldAutoGenerateSlug()
    {
        // Act
        var category = Category.Create("Electronics");
        
        // Assert
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("electronics", category.Slug); // Auto-generated
        Assert.True(category.IsActive);
        Assert.False(category.IsDeleted);
        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.NotEqual(default, category.CreatedAt);
    }
    
    [Fact]
    public void Create_WithNameAndSlug_ShouldUseProvidedSlug()
    {
        // Act
        var category = Category.Create("Electronics", "electronic-devices");
        
        // Assert
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("electronic-devices", category.Slug);
    }
    
    [Fact]
    public void Create_WithSpecialCharactersInName_ShouldGenerateValidSlug()
    {
        // Act
        var category = Category.Create("Electronics & Gadgets!");
        
        // Assert
        Assert.Equal("Electronics & Gadgets!", category.Name);
        Assert.Equal("electronics-and-gadgets", category.Slug);
    }
    
    [Fact]
    public void Create_WithAccentedCharacters_ShouldGenerateSlugWithoutAccents()
    {
        // Act
        var category = Category.Create("Tecnología Electrónica");
        
        // Assert
        Assert.Equal("Tecnología Electrónica", category.Name);
        Assert.Equal("tecnologia-electronica", category.Slug);
    }
    
    [Fact]
    public void Create_WithUpperCaseName_ShouldGenerateLowerCaseSlug()
    {
        // Act
        var category = Category.Create("ELECTRONICS");
        
        // Assert
        Assert.Equal("ELECTRONICS", category.Name);
        Assert.Equal("electronics", category.Slug);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowException(string? name)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create(name));
    }
    
    [Theory]
    [InlineData("", "electronics")]
    [InlineData(" ", "electronics")]
    [InlineData(null, "electronics")]
    [InlineData("Electronics", "")]
    [InlineData("Electronics", " ")]
    [InlineData("Electronics", null)]
    [InlineData("Electronics", "invalid slug with spaces")]
    [InlineData("Electronics", "invalid--slug")]
    [InlineData("Electronics", "invalid__slug")]
    [InlineData("Electronics", "UPPERCASE")]
    [InlineData("Electronics", "Special@Char")]
    public void Create_WithInvalidSlug_ShouldThrowException(string? name, string slug)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create(name, slug));
    }
    
    [Fact]
    public void Create_WithSlugThatBecomesEmptyAfterGeneration_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Category.Create("!!!"));
    }
    
    [Fact]
    public void Update_WithNameOnly_ShouldRegenerateSlugFromName()
    {
        // Arrange
        var category = Category.Create("Old Name", "old-name");
        var originalUpdatedAt = category.UpdatedAt;
        
        // Act
        category.Update("New Name");
        
        // Assert
        Assert.Equal("New Name", category.Name);
        Assert.Equal("new-name", category.Slug);
        Assert.NotNull(category.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, category.UpdatedAt);
    }
    
    [Fact]
    public void Update_WithNameAndSlug_ShouldUseProvidedSlug()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var originalUpdatedAt = category.UpdatedAt;
        
        // Act
        category.Update("Computers", "computers-category");
        
        // Assert
        Assert.Equal("Computers", category.Name);
        Assert.Equal("computers-category", category.Slug);
        Assert.NotNull(category.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, category.UpdatedAt);
    }
    
    [Fact]
    public void Update_WithSameName_ShouldNotChangeSlug()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var originalSlug = category.Slug;
        
        // Act
        category.Update("Electronics", "electronics");
        
        // Assert
        Assert.Equal("Electronics", category.Name);
        Assert.Equal(originalSlug, category.Slug);
    }
    
    [Fact]
    public void Update_WithInvalidName_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => category.Update(""));
        Assert.Throws<ArgumentException>(() => category.Update(null!));
    }
    
    [Fact]
    public void UpdateSlug_ShouldUpdateOnlySlug()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var originalName = category.Name;
        var originalUpdatedAt = category.UpdatedAt;
        
        // Act
        category.UpdateSlug("electronic-devices");
        
        // Assert
        Assert.Equal(originalName, category.Name);
        Assert.Equal("electronic-devices", category.Slug);
        Assert.NotNull(category.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, category.UpdatedAt);
    }
    
    [Fact]
    public void UpdateSlug_WithInvalidSlug_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => category.UpdateSlug(""));
        Assert.Throws<ArgumentException>(() => category.UpdateSlug(null!));
        Assert.Throws<ArgumentException>(() => category.UpdateSlug("invalid slug"));
        Assert.Throws<ArgumentException>(() => category.UpdateSlug("invalid--slug"));
    }
    
    [Fact]
    public void SoftDelete_ShouldMarkAsDeletedAndInactive()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        Assert.True(category.IsActive);
        Assert.False(category.IsDeleted);
        
        // Act
        category.SoftDelete();
        
        // Assert
        Assert.True(category.IsDeleted);
        Assert.False(category.IsActive);
        Assert.NotNull(category.UpdatedAt);
    }
    
    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        Assert.True(category.IsActive);
        
        // Act
        category.Deactivate();
        
        // Assert
        Assert.False(category.IsActive);
        Assert.False(category.IsDeleted);
        Assert.NotNull(category.UpdatedAt);
    }
    
    [Fact]
    public void Activate_ShouldSetActiveTrue()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        category.Deactivate();
        Assert.False(category.IsActive);
        
        // Act
        category.Activate();
        
        // Assert
        Assert.True(category.IsActive);
        Assert.NotNull(category.UpdatedAt);
    }
    
    [Fact]
    public void Restore_ShouldRestoreSoftDeletedCategory()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        category.SoftDelete();
        Assert.True(category.IsDeleted);
        Assert.False(category.IsActive);
        
        // Act
        category.Restore();
        
        // Assert
        Assert.False(category.IsDeleted);
        Assert.True(category.IsActive);
        Assert.NotNull(category.UpdatedAt);
    }
    
    [Fact]
    public void AddProduct_ShouldAddProductToCollection()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var product = Product.Create("Laptop", category.Id, price: 1299.99m, stock: 10);
        
        // Act
        category.AddProduct(product);
        
        // Assert
        Assert.Contains(product, category.Products);
        Assert.Single(category.Products);
    }
    
    [Fact]
    public void AddProduct_WithNullProduct_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => category.AddProduct(null!));
    }
    
    [Fact]
    public void AddMultipleProducts_ShouldAddAllToCollection()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var product1 = Product.Create("Laptop", category.Id, price: 1299.99m, stock: 10);
        var product2 = Product.Create("Mouse", category.Id, price: 29.99m, stock: 50);
        var product3 = Product.Create("Keyboard", category.Id, price: 89.99m, stock: 25);
        
        // Act
        category.AddProduct(product1);
        category.AddProduct(product2);
        category.AddProduct(product3);
        
        // Assert
        Assert.Equal(3, category.Products.Count);
        Assert.Contains(product1, category.Products);
        Assert.Contains(product2, category.Products);
        Assert.Contains(product3, category.Products);
    }
    
    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Act
        var category = Category.Create("Electronics", "electronics");
        
        // Assert
        Assert.True(category.IsActive);
        Assert.False(category.IsDeleted);
        Assert.Equal(default, category.CreatedBy);
        Assert.Null(category.UpdatedBy);
        Assert.Null(category.UpdatedAt);
        Assert.Empty(category.Products);
    }
    
    [Fact]
    public void Update_ShouldUpdateTimestamps()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var originalUpdatedAt = category.UpdatedAt;
        
        // Wait a moment to ensure time difference
        System.Threading.Thread.Sleep(10);
        
        // Act
        category.Update("Computers", "computers");
        
        // Assert
        Assert.NotNull(category.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, category.UpdatedAt);
        Assert.True(category.UpdatedAt > originalUpdatedAt);
    }
    
    [Fact]
    public void MultipleUpdates_ShouldUpdateTimestampsEachTime()
    {
        // Arrange
        var category = Category.Create("Electronics", "electronics");
        var firstUpdate = category.UpdatedAt;
        
        System.Threading.Thread.Sleep(10);
        category.Update("Computers", "computers");
        var secondUpdate = category.UpdatedAt;
        
        System.Threading.Thread.Sleep(10);
        category.UpdateSlug("computer-devices");
        var thirdUpdate = category.UpdatedAt;
        
        // Assert
        Assert.NotEqual(firstUpdate, secondUpdate);
        Assert.NotEqual(secondUpdate, thirdUpdate);
        Assert.True(secondUpdate > firstUpdate);
        Assert.True(thirdUpdate > secondUpdate);
    }
    
    [Fact]
    public void Create_WithVeryLongName_ShouldGenerateTruncatedSlug()
    {
        // Arrange
        var veryLongName = new string('a', 300);
        
        // Act
        var category = Category.Create(veryLongName);
        
        // Assert
        Assert.True(category.Slug.Length <= 200);
        Assert.EndsWith("a", category.Slug);
    }
    
    [Theory]
    [InlineData("Café", "cafe")]
    [InlineData("Niño", "nino")]
    [InlineData("Straße", "strasse")]
    [InlineData("Müller", "muller")]
    [InlineData("François", "francois")]
    public void Create_WithAccentedNames_ShouldGenerateCorrectSlug(string? name, string expectedSlug)
    {
        // Act
        var category = Category.Create(name);
        
        // Assert
        Assert.Equal(expectedSlug, category.Slug);
    }
    
    [Theory]
    [InlineData("Home & Garden", "home-and-garden")]
    [InlineData("Books & Literature", "books-and-literature")]
    [InlineData("Clothing & Apparel", "clothing-and-apparel")]
    [InlineData("Sports & Outdoors", "sports-and-outdoors")]
    public void Create_WithSpecialCharacters_ShouldGenerateCorrectSlug(string? name, string expectedSlug)
    {
        // Act
        var category = Category.Create(name);
        
        // Assert
        Assert.Equal(expectedSlug, category.Slug);
    }
}