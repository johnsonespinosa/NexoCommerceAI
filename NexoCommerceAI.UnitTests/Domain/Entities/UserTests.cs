using NexoCommerceAI.Domain.Entities;
using Xunit;
using Assert = Xunit.Assert;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class UserTests
{
    private readonly Guid _roleId = Guid.NewGuid();
    private const string PasswordHash = "hashed_password_123";

    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Act
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        
        // Assert
        Assert.Equal("john_doe", user.UserName);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal(PasswordHash, user.PasswordHash);
        Assert.Equal(_roleId, user.RoleId);
        Assert.True(user.IsActive);
        Assert.False(user.IsDeleted);
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
    }
    
    [Theory]
    [InlineData("", "john@example.com")]
    [InlineData(null, "john@example.com")]
    [InlineData("jo", "john@example.com")] // Muy corto
    [InlineData("verylongusernameexceedingfiftycharacterslimit", "john@example.com")] // Muy largo
    [InlineData("john_doe", "")]
    [InlineData("john_doe", null)]
    [InlineData("john_doe", "invalid-email")]
    [InlineData("john_doe", "missing@domain")]
    public void Create_WithInvalidData_ShouldThrowException(string userName, string email)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            User.Create(userName, email, PasswordHash, _roleId));
    }
    
    [Fact]
    public void Create_WithEmptyPasswordHash_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            User.Create("john_doe", "john@example.com", "", _roleId));
    }
    
    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var originalUpdatedAt = user.UpdatedAt;
        
        // Act
        user.UpdateProfile("johndoe_updated", "john.updated@example.com");
        
        // Assert
        Assert.Equal("johndoe_updated", user.UserName);
        Assert.Equal("john.updated@example.com", user.Email);
        Assert.NotNull(user.UpdatedAt);
        Assert.NotEqual(originalUpdatedAt, user.UpdatedAt);
    }
    
    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var newPasswordHash = "new_hashed_password_456";
        
        // Act
        user.UpdatePassword(newPasswordHash);
        
        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void UpdateRole_WithValidRoleId_ShouldUpdateRole()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var newRoleId = Guid.NewGuid();
        
        // Act
        user.UpdateRole(newRoleId);
        
        // Assert
        Assert.Equal(newRoleId, user.RoleId);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void UpdateRole_WithEmptyGuid_ShouldThrowException()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.UpdateRole(Guid.Empty));
    }
    
    [Fact]
    public void SetRefreshToken_WithValidData_ShouldSetToken()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var refreshToken = "valid_refresh_token_123";
        var expiryTime = DateTime.UtcNow.AddDays(7);
        
        // Act
        user.SetRefreshToken(refreshToken, expiryTime);
        
        // Assert
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(expiryTime, user.RefreshTokenExpiryTime);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void SetRefreshToken_WithPastExpiry_ShouldThrowException()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var refreshToken = "valid_refresh_token_123";
        var expiryTime = DateTime.UtcNow.AddDays(-1);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            user.SetRefreshToken(refreshToken, expiryTime));
    }
    
    [Fact]
    public void IsRefreshTokenValid_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        var refreshToken = "valid_refresh_token_123";
        var expiryTime = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, expiryTime);
        
        // Act
        var isValid = user.IsRefreshTokenValid(refreshToken);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Theory]
    [InlineData("wrong_token")]
    [InlineData("")]
    [InlineData(null)]
    public void IsRefreshTokenValid_WithInvalidToken_ShouldReturnFalse(string token)
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        const string refreshToken = "valid_refresh_token_123";
        var expiryTime = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, expiryTime);
        
        // Act
        var isValid = user.IsRefreshTokenValid(token);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void RevokeRefreshToken_ShouldClearToken()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        user.SetRefreshToken("token", DateTime.UtcNow.AddDays(7));
        
        // Act
        user.RevokeRefreshToken();
        
        // Assert
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        
        // Act
        user.Deactivate();
        
        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void Activate_ShouldSetActiveTrue()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        user.Deactivate();
        
        // Act
        user.Activate();
        
        // Assert
        Assert.True(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
    
    [Fact]
    public void SoftDelete_ShouldMarkAsDeletedAndInactive()
    {
        // Arrange
        var user = User.Create("john_doe", "john@example.com", PasswordHash, _roleId);
        
        // Act
        user.SoftDelete();
        
        // Assert
        Assert.True(user.IsDeleted);
        Assert.False(user.IsActive);
        Assert.NotNull(user.UpdatedAt);
    }
}