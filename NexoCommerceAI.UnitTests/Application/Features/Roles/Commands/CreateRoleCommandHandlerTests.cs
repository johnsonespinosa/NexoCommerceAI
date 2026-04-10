using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Commands;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly CreateRoleCommandHandler _handler;

    public CreateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<CreateRoleCommandHandler>> loggerMock = new();
        
        _handler = new CreateRoleCommandHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateRole_WhenValid()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "Moderator",
            Description = "Moderator with limited permissions"
        };

        _roleRepositoryMock.Setup(r => r.IsRoleNameUniqueAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
        Assert.True(result.IsActive);
        Assert.Equal(0, result.UserCount);
        
        _roleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenRoleNameAlreadyExists()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "Admin",
            Description = "Administrator role"
        };

        _roleRepositoryMock.Setup(r => r.IsRoleNameUniqueAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _roleRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "",
            Description = "Test description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}