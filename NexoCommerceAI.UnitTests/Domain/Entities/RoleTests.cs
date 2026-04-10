using NexoCommerceAI.Domain.Entities;
using Xunit;
using Assert = Xunit.Assert;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class RoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRole()
    {
        // Act
        var role = Role.Create("Admin", "Administrator role");
        
        // Assert
        Assert.Equal("Admin", role.Name);
        Assert.Equal("Administrator role", role.Description);
        Assert.True(role.IsActive);
        Assert.False(role.IsDeleted);
    }
    
    [Xunit.Theory]
    [InlineData("", "Description")]
    [InlineData(null, "Description")]
    [InlineData("Admin", "")]
    [InlineData("Admin", null)]
    public void Create_WithInvalidData_ShouldThrowException(string? name, string? description)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Role.Create(name!, description!));
    }
    
    [Fact]
    public void Update_WithValidData_ShouldUpdateRole()
    {
        // Arrange
        var role = Role.Create("Admin", "Administrator");
        var originalUpdatedAt = role.UpdatedAt;
        
        // Act
        role.Update("SuperAdmin", "Super administrator role");
        
        // Assert
        Assert.Equal("SuperAdmin", role.Name);
        Assert.Equal("Super administrator role", role.Description);
        Assert.NotNull(role.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, role.UpdatedAt);
    }
    
    [Fact]
    public void SoftDelete_ShouldMarkAsDeletedAndInactive()
    {
        // Arrange
        var role = Role.Create("Admin", "Administrator");
        
        // Act
        role.SoftDelete();
        
        // Assert
        Assert.True(role.IsDeleted);
        Assert.False(role.IsActive);
    }
}