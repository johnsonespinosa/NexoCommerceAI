using System.Security.Claims;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Username { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    ClaimsPrincipal? User { get; }
}