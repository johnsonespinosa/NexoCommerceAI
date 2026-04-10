using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class UserRepositoryTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private readonly UserRepository _repository = new(fixture.Context);

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUserWhenExists()
    {
        // Act
        var result = await _repository.GetByEmailAsync("admin@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("admin", result.UserName);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_ShouldReturnTrueForUniqueEmail()
    {
        // Act
        var isUnique = await _repository.IsEmailUniqueAsync("unique@test.com", null);

        // Assert
        Assert.True(isUnique);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_ShouldReturnFalseForExistingEmail()
    {
        // Act
        var isUnique = await _repository.IsEmailUniqueAsync("admin@test.com", null);

        // Assert
        Assert.False(isUnique);
    }

    [Fact]
    public async Task GetByIdWithRoleAsync_ShouldReturnUserWithRole()
    {
        // Arrange
        var user = await _repository.GetByEmailAsync("admin@test.com");

        // Act
        var result = await _repository.GetByIdWithRoleAsync(user!.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Role);
        Assert.Equal("Admin", result.Role.Name);
    }
}