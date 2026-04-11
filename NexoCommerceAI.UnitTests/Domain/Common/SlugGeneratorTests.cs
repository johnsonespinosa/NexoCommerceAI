using NexoCommerceAI.Domain.Common;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Common;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Hello   World  ", "hello-world")]
    [InlineData("Café & Tea", "cafe-and-tea")]
    [InlineData("El Niño", "el-nino")]
    [InlineData("100% Pure", "100-pure")]
    [InlineData("Hello@World", "hello-at-world")]
    [InlineData("Hello#World", "hello-sharp-world")]
    [InlineData("¿Qué pasa?", "que-pasa")]
    [InlineData("¡Hola! ¿Cómo estás?", "hola-como-estas")]
    [InlineData("Product (New) [2024]", "product-new-2024")]
    [InlineData("   ", "untitled")] // Input vacío después de trim
    [InlineData("Übermensch", "ubermensch")]
    [InlineData("Straße", "strasse")]
    public void Generate_ShouldCreateValidSlug(string? input, string expectedSlug)
    {
        // Act
        var slug = SlugGenerator.Generate(input);
        
        // Assert
        Assert.Equal(expectedSlug, slug);
    }
    
    [Fact]
    public void Generate_WithLowercaseFalse_ShouldPreserveCase()
    {
        // Act
        var slug = SlugGenerator.Generate("Hello World", lowerCase: false);
        
        // Assert
        Assert.Equal("Hello-World", slug);
    }
    
    [Fact]
    public void Generate_WithPreserveNumbersFalse_ShouldRemoveNumbers()
    {
        // Act
        var slug = SlugGenerator.Generate("Product123 v4", preserveNumbers: false);
        
        // Assert
        Assert.Equal("product-v", slug);
    }
    
    [Fact]
    public void Generate_WithVeryLongInput_ShouldTruncate()
    {
        // Arrange
        var longInput = new string('a', 300);
        
        // Act
        var slug = SlugGenerator.Generate(longInput);
        
        // Assert
        Assert.True(slug.Length <= 200);
        Assert.EndsWith("a", slug);
    }
    
    [Fact]
    public void Generate_WithNullInput_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SlugGenerator.Generate(null!));
    }
    
    [Fact]
    public void GenerateUnique_ShouldAddNumberWhenSlugExists()
    {
        // Arrange
        var existingSlugs = new HashSet<string> { "laptop", "laptop-1" };
        
        // Act
        var slug = SlugGenerator.GenerateUnique("Laptop", slug => existingSlugs.Contains(slug));
        
        // Assert
        Assert.Equal("laptop-2", slug);
    }
    
    [Fact]
    public void GenerateUnique_ShouldReturnOriginalSlugWhenAvailable()
    {
        // Arrange
        var existingSlugs = new HashSet<string> { "laptop-1" };
        
        // Act
        var slug = SlugGenerator.GenerateUnique("Laptop", slug => existingSlugs.Contains(slug));
        
        // Assert
        Assert.Equal("laptop", slug);
    }
    
    [Fact]
    public void GenerateUnique_ShouldThrowExceptionWhenMaxAttemptsExceeded()
    {
        // Arrange
        var existingSlugs = new HashSet<string>();
        for (var i = 0; i <= 10; i++)
        {
            existingSlugs.Add($"laptop-{(i == 0 ? "" : i.ToString())}");
        }
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            SlugGenerator.GenerateUnique("Laptop", slug => existingSlugs.Contains(slug), maxAttempts: 10));
    }
}