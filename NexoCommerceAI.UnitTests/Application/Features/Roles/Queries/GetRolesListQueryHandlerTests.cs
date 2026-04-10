using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Application.Features.Roles.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Queries;

public class GetRolesListQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly GetRolesListQueryHandler _handler;

    public GetRolesListQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<GetRolesListQueryHandler>> loggerMock = new();
        
        _handler = new GetRolesListQueryHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenRolesExist()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var query = new GetRolesListQuery { Pagination = pagination };
        var roles = new List<Role>
        {
            Role.Create("Admin", "Administrator"),
            Role.Create("Customer", "Regular customer")
        };

        _roleRepositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldFilterRoles_WhenSearchTermProvided()
    {
        // Arrange
        var pagination = new PaginationParams 
        { 
            PageNumber = 1, 
            PageSize = 10,
            SearchTerm = "Admin"
        };
        var query = new GetRolesListQuery { Pagination = pagination };
        var roles = new List<Role>
        {
            Role.Create("Admin", "Administrator"),
            Role.Create("Customer", "Regular customer"),
            Role.Create("SuperAdmin", "Super administrator")
        };

        _roleRepositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, r => r.Name == "Admin");
        Assert.Contains(result.Items, r => r.Name == "SuperAdmin");
        Assert.DoesNotContain(result.Items, r => r.Name == "Customer");
    }
}